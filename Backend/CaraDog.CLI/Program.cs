using System.Globalization;
using System.Text.Json;
using CaraDog.Db;
using CaraDog.Db.Entities;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("CaraDog CLI");

if (args.Length == 0 || args.Contains("--help", StringComparer.OrdinalIgnoreCase))
{
    PrintUsage();
    return;
}

if (args.Contains("--version", StringComparer.OrdinalIgnoreCase))
{
    var version = typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown";
    Console.WriteLine($"Version: {version}");
    return;
}

if (IsSeedCommand(args))
{
    var filePath = GetOptionValue(args, "--file") ?? GetOptionValue(args, "-f");
    if (string.IsNullOrWhiteSpace(filePath))
    {
        Console.WriteLine("Missing required option: --file <path>");
        return;
    }

    if (!File.Exists(filePath))
    {
        Console.WriteLine($"Seed file not found: {filePath}");
        return;
    }

    var connectionString = GetOptionValue(args, "--connection")
        ?? Environment.GetEnvironmentVariable("CARADOG_CONNECTION")
        ?? "server=localhost;port=3306;database=caradog;user=app;password=apppw;";

    var upsert = args.Contains("--upsert", StringComparer.OrdinalIgnoreCase);

    var json = await File.ReadAllTextAsync(filePath);
    var seedData = JsonSerializer.Deserialize<SeedData>(json, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });

    if (seedData is null)
    {
        Console.WriteLine("Seed file is empty or invalid JSON.");
        return;
    }

    var options = new DbContextOptionsBuilder<CaraDogDbContext>()
        .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
        .Options;

    await using var db = new CaraDogDbContext(options);

    var existingCategories = await db.Categories.ToListAsync();
    var categoriesByName = existingCategories
        .ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);

    var categoryCreates = 0;
    var categoryUpdates = 0;
    var categorySeeds = seedData.Categories ?? new List<CategorySeed>();
    foreach (var category in categorySeeds)
    {
        if (categoriesByName.TryGetValue(category.Name, out var existing))
        {
            if (upsert)
            {
                existing.Description = category.Description;
                categoryUpdates++;
            }
            continue;
        }

        var newCategory = new Category
        {
            Id = Guid.NewGuid(),
            Name = category.Name,
            Description = category.Description
        };
        db.Categories.Add(newCategory);
        categoriesByName[newCategory.Name] = newCategory;
        categoryCreates++;
    }

    if (categoryCreates > 0 || categoryUpdates > 0)
    {
        await db.SaveChangesAsync();
    }

    var categoriesReloaded = await db.Categories.ToListAsync();
    var categoryLookup = categoriesReloaded.ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);

    var existingTags = await db.Tags.ToListAsync();
    var tagsByName = existingTags
        .ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

    var tagCreates = 0;
    var tagUpdates = 0;
    var tagSeeds = seedData.Tags ?? new List<TagSeed>();
    foreach (var tag in tagSeeds)
    {
        if (tagsByName.TryGetValue(tag.Name, out var existing))
        {
            if (upsert)
            {
                existing.Name = tag.Name;
                tagUpdates++;
            }
            continue;
        }

        var newTag = new Tag
        {
            Id = Guid.NewGuid(),
            Name = tag.Name
        };
        db.Tags.Add(newTag);
        tagsByName[newTag.Name] = newTag;
        tagCreates++;
    }

    if (tagCreates > 0 || tagUpdates > 0)
    {
        await db.SaveChangesAsync();
    }

    var tagsReloaded = await db.Tags.ToListAsync();
    var tagLookup = tagsReloaded.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

    var existingProducts = await db.Products.ToListAsync();
    var productsBySku = existingProducts
        .ToDictionary(p => p.Sku, StringComparer.OrdinalIgnoreCase);

    var productCreates = 0;
    var productUpdates = 0;
    var productSeeds = seedData.Products ?? new List<ProductSeed>();
    foreach (var product in productSeeds)
    {
        if (!categoryLookup.TryGetValue(product.CategoryName, out var category))
        {
            Console.WriteLine($"Skipping product '{product.Name}'. Unknown category: {product.CategoryName}");
            continue;
        }

        if (productsBySku.TryGetValue(product.Sku, out var existing))
        {
            if (upsert)
            {
                existing.Name = product.Name;
                existing.Description = product.Description;
                existing.NetPrice = product.NetPrice;
                existing.IsSoldOut = product.IsSoldOut;
                existing.CategoryId = category.Id;
                productUpdates++;
            }
            continue;
        }

        var newProduct = new Product
        {
            Id = Guid.NewGuid(),
            Name = product.Name,
            Description = product.Description,
            Sku = product.Sku,
            NetPrice = product.NetPrice,
            IsSoldOut = product.IsSoldOut,
            CategoryId = category.Id
        };

        db.Products.Add(newProduct);
        productsBySku[newProduct.Sku] = newProduct;
        productCreates++;
    }

    if (productCreates > 0 || productUpdates > 0)
    {
        await db.SaveChangesAsync();
    }

    var existingProductTags = await db.ProductTags.ToListAsync();
    var productTagKeys = new HashSet<(Guid ProductId, Guid TagId)>(
        existingProductTags.Select(pt => (pt.ProductId, pt.TagId)));

    var productTagCreates = 0;
    var productTagSeeds = seedData.ProductTags ?? new List<ProductTagSeed>();
    foreach (var productTag in productTagSeeds)
    {
        if (!productsBySku.TryGetValue(productTag.ProductSku, out var product))
        {
            Console.WriteLine($"Skipping tag for unknown SKU: {productTag.ProductSku}");
            continue;
        }

        if (!tagLookup.TryGetValue(productTag.TagName, out var tag))
        {
            Console.WriteLine($"Skipping unknown tag: {productTag.TagName}");
            continue;
        }

        var key = (product.Id, tag.Id);
        if (productTagKeys.Contains(key))
        {
            continue;
        }

        var newProductTag = new ProductTag
        {
            ProductId = product.Id,
            TagId = tag.Id
        };
        db.ProductTags.Add(newProductTag);
        productTagKeys.Add(key);
        productTagCreates++;
    }

    if (productTagCreates > 0)
    {
        await db.SaveChangesAsync();
    }

    var productsReloaded = await db.Products.ToListAsync();
    var productLookup = productsReloaded.ToDictionary(p => p.Sku, StringComparer.OrdinalIgnoreCase);
    var inventoriesByProductId = await db.Inventories.ToDictionaryAsync(i => i.ProductId);

    var inventoryCreates = 0;
    var inventoryUpdates = 0;
    var inventorySeeds = seedData.Inventories ?? new List<InventorySeed>();
    foreach (var inventory in inventorySeeds)
    {
        if (!productLookup.TryGetValue(inventory.ProductSku, out var product))
        {
            Console.WriteLine($"Skipping inventory for unknown SKU: {inventory.ProductSku}");
            continue;
        }

        if (inventoriesByProductId.TryGetValue(product.Id, out var existing))
        {
            if (upsert)
            {
                existing.Quantity = inventory.Quantity;
                existing.UpdatedAt = DateTime.UtcNow;
                inventoryUpdates++;
            }
            continue;
        }

        var newInventory = new Inventory
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Quantity = inventory.Quantity,
            UpdatedAt = DateTime.UtcNow
        };
        db.Inventories.Add(newInventory);
        inventoriesByProductId[product.Id] = newInventory;
        inventoryCreates++;
    }

    if (inventoryCreates > 0 || inventoryUpdates > 0)
    {
        await db.SaveChangesAsync();
    }

    Console.WriteLine("Seed completed.");
    Console.WriteLine($"Categories: +{categoryCreates}, ~{categoryUpdates}");
    Console.WriteLine($"Tags:       +{tagCreates}, ~{tagUpdates}");
    Console.WriteLine($"Products:   +{productCreates}, ~{productUpdates}");
    Console.WriteLine($"Prod tags:  +{productTagCreates}");
    Console.WriteLine($"Inventory:  +{inventoryCreates}, ~{inventoryUpdates}");
    return;
}

Console.WriteLine($"Started at {DateTime.UtcNow.ToString("u", CultureInfo.InvariantCulture)}");
PrintUsage();

static bool IsSeedCommand(string[] args)
    => args.Length > 0
       && (args[0].Equals("seed", StringComparison.OrdinalIgnoreCase)
           || args.Contains("--seed", StringComparer.OrdinalIgnoreCase));

static string? GetOptionValue(string[] args, string option)
{
    for (var i = 0; i < args.Length; i++)
    {
        var arg = args[i];
        if (arg.Equals(option, StringComparison.OrdinalIgnoreCase))
        {
            if (i + 1 < args.Length)
            {
                return args[i + 1];
            }
        }

        if (arg.StartsWith(option + "=", StringComparison.OrdinalIgnoreCase))
        {
            return arg[(option.Length + 1)..];
        }
    }

    return null;
}

static void PrintUsage()
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  caradog.cli --version");
    Console.WriteLine("  caradog.cli seed --file <path> [--connection <connStr>] [--upsert]");
    Console.WriteLine();
    Console.WriteLine("Notes:");
    Console.WriteLine("  - Connection string default matches the API.");
    Console.WriteLine("  - You can also set CARADOG_CONNECTION env var.");
    Console.WriteLine("  - Example seed file: Backend/CaraDog.CLI/seed-example.json");
}

sealed class SeedData
{
    public List<CategorySeed> Categories { get; init; } = new();
    public List<TagSeed> Tags { get; init; } = new();
    public List<ProductSeed> Products { get; init; } = new();
    public List<ProductTagSeed> ProductTags { get; init; } = new();
    public List<InventorySeed> Inventories { get; init; } = new();
}

sealed class CategorySeed
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

sealed class ProductSeed
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Sku { get; init; } = string.Empty;
    public decimal NetPrice { get; init; }
    public bool IsSoldOut { get; init; }
    public string CategoryName { get; init; } = string.Empty;
}

sealed class TagSeed
{
    public string Name { get; init; } = string.Empty;
}

sealed class ProductTagSeed
{
    public string ProductSku { get; init; } = string.Empty;
    public string TagName { get; init; } = string.Empty;
}

sealed class InventorySeed
{
    public string ProductSku { get; init; } = string.Empty;
    public int Quantity { get; init; }
}

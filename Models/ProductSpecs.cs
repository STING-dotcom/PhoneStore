namespace PhoneStore.Models;

public class ProductSpecs
{
    public string Screen { get; set; } = "6.7 inches OLED";
    public string Os { get; set; } = "Android 14";
    public string Cpu { get; set; } = "Snapdragon 8 Gen 3";
    public string Ram { get; set; } = "8GB";
    public string Storage { get; set; } = "256GB";
    public string Battery { get; set; } = "5000 mAh";
    public string Camera { get; set; } = "50MP + 12MP + 8MP";
    public string Weight { get; set; } = "200g";

    public static ProductSpecs Generate(string brand, string name, decimal price)
    {
        var specs = new ProductSpecs();
        var isHighEnd = price >= 15000000;
        var isMid = price >= 7000000;

        // === OS ===
        specs.Os = brand == "Apple" ? "iOS 18" : "Android 14";

        // === CPU ===
        specs.Cpu = brand switch
        {
            "Apple" when isHighEnd => "Apple A18 Pro",
            "Apple" => "Apple A16 Bionic",
            "Samsung" when isHighEnd => "Snapdragon 8 Gen 3 / Exynos 2400",
            "Samsung" when isMid => "Exynos 1480",
            "Samsung" => "Exynos 1380",
            "Xiaomi" when isHighEnd => "Snapdragon 8 Gen 3",
            "Xiaomi" when isMid => "Dimensity 7200 Ultra",
            "Xiaomi" => "Helio G99",
            "OPPO" when isHighEnd => "Snapdragon 8 Gen 2",
            "OPPO" when isMid => "Dimensity 7300",
            "OPPO" => "Helio G88",
            _ when isHighEnd => "Snapdragon 8 Gen 3",
            _ when isMid => "Snapdragon 7 Gen 3",
            _ => "Snapdragon 680"
        };

        // === Screen ===
        if (brand == "Apple")
        {
            if (name.Contains("Pro Max")) specs.Screen = "6.9 inches OLED Super Retina XDR, 120Hz";
            else if (name.Contains("Pro")) specs.Screen = "6.3 inches OLED Super Retina XDR, 120Hz";
            else if (name.Contains("Plus")) specs.Screen = "6.7 inches OLED, 60Hz";
            else specs.Screen = "6.1 inches OLED, 60Hz";
        }
        else
        {
            if (isHighEnd) specs.Screen = "6.8 inches AMOLED 2K, 120Hz LTPO";
            else if (isMid) specs.Screen = "6.7 inches AMOLED FHD+, 120Hz";
            else specs.Screen = "6.6 inches IPS LCD, 90Hz";
        }

        // === RAM & Storage ===
        if (isHighEnd)
        {
            specs.Ram = name.Contains("Ultra") || name.Contains("Pro Max") ? "16GB" : "12GB";
            specs.Storage = name.Contains("512") || name.Contains("1TB") ? "512GB" : "256GB";
        }
        else if (isMid)
        {
            specs.Ram = "8GB";
            specs.Storage = name.Contains("256") ? "256GB" : "128GB";
        }
        else
        {
            specs.Ram = "6GB";
            specs.Storage = "128GB";
        }

        // === Battery ===
        if (brand == "Apple")
        {
            if (name.Contains("Pro Max")) specs.Battery = "4685 mAh, 25W";
            else if (name.Contains("Pro")) specs.Battery = "3650 mAh, 20W";
            else if (name.Contains("Plus")) specs.Battery = "4383 mAh, 20W";
            else specs.Battery = "3349 mAh, 20W";
        }
        else
        {
            if (isHighEnd) specs.Battery = "5000 mAh, 65W";
            else if (isMid) specs.Battery = "5000 mAh, 45W";
            else specs.Battery = "5000 mAh, 18W";
        }

        // === Camera ===
        if (isHighEnd)
        {
            if (name.Contains("Ultra")) specs.Camera = "200MP + 50MP + 12MP + 10MP";
            else if (name.Contains("Pro")) specs.Camera = "50MP + 50MP + 64MP + 8MP";
            else specs.Camera = "50MP + 12MP + 12MP";
        }
        else if (isMid)
        {
            specs.Camera = "108MP + 8MP + 2MP";
        }
        else
        {
            specs.Camera = "50MP + 2MP + 2MP";
        }

        // === Weight ===
        if (name.Contains("Fold")) specs.Weight = "253g";
        else if (name.Contains("Ultra")) specs.Weight = "220g";
        else if (name.Contains("Pro Max")) specs.Weight = "225g";
        else if (name.Contains("Pro")) specs.Weight = "190g";
        else if (name.Contains("Plus")) specs.Weight = "200g";
        else if (isHighEnd) specs.Weight = "210g";
        else if (isMid) specs.Weight = "195g";
        else specs.Weight = "190g";

        return specs;
    }
}

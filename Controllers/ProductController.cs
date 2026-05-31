using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhoneStore.Data;
using PhoneStore.Models;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace PhoneStore.Controllers;

public class ProductController : Controller
{
    private readonly AppDbContext _db;

    public ProductController(AppDbContext db) => _db = db;

    internal static readonly List<Product> _products = new List<Product>
    {
        new Product { Id = 1, Name = "iPhone 15 Pro Max 256GB", Brand = "Apple", Price = 29490000, ImageUrl = "/images/ProductsApple/iphone15-promax.jpg", Description = "Cao cấp - Titan tự nhiên quyền lực", SourceUrl = "https://fptshop.com.vn/dien-thoai/iphone-15-pro-max" },
        new Product { Id = 2, Name = "iPhone 15 Pro 128GB", Brand = "Apple", Price = 24990000, ImageUrl = "/images/ProductsApple/Iphone15pro.jpg", Description = "Cao cấp - Hiệu năng đỉnh cao nhỏ gọn", SourceUrl = "https://fptshop.com.vn/dien-thoai/iphone-15-pro" },
        new Product { Id = 3, Name = "iPhone 15 Plus 128GB", Brand = "Apple", Price = 22490000, ImageUrl = "/images/ProductsApple/iphone15plus.jpg", Description = "Cao cấp - Màn hình lớn, pin trâu kỷ lục", SourceUrl = "https://fptshop.com.vn/dien-thoai/iphone-15-plus" },
        new Product { Id = 4, Name = "iPhone 15 128GB", Brand = "Apple", Price = 19790000, ImageUrl = "/images/ProductsApple/iphone15.jpg", Description = "Cao cấp - Dynamic Island thời thượng", SourceUrl = "https://fptshop.com.vn/dien-thoai/iphone-15" },
        new Product { Id = 5, Name = "iPhone 14 Pro Max 128GB", Brand = "Apple", Price = 26500000, ImageUrl = "/images/ProductsApple/iphone14promax.jpg", Description = "Cao cấp - Cựu vương màn hình siêu đẹp" },
        new Product { Id = 6, Name = "iPhone 14 128GB", Brand = "Apple", Price = 16990000, ImageUrl = "/images/ProductsApple/iphone14.jpg", Description = "Tầm trung - Trải nghiệm mượt mà giá tốt", SourceUrl = "https://fptshop.com.vn/dien-thoai/iphone-14" },
        new Product { Id = 7, Name = "iPhone 13 128GB", Brand = "Apple", Price = 13590000, ImageUrl = "/images/ProductsApple/iphone13.jpg", Description = "Tầm trung - Chiếc iPhone quốc dân trường tồn", SourceUrl = "https://fptshop.com.vn/dien-thoai/iphone-13" },
        new Product { Id = 8, Name = "iPhone 12 64GB", Brand = "Apple", Price = 11490000, ImageUrl = "/images/ProductsApple/iphone12.jpg", Description = "Tầm trung - Thiết kế vuông vức thời trang", SourceUrl = "https://fptshop.com.vn/dien-thoai/iphone-12" },
        new Product { Id = 9, Name = "iPhone 11 64GB", Brand = "Apple", Price = 8490000, ImageUrl = "/images/ProductsApple/iphone11.jpg", Description = "Giá rẻ - Máy phụ hoàn hảo cho học sinh", SourceUrl = "https://fptshop.com.vn/dien-thoai/iphone-11" },
        new Product { Id = 10, Name = "iPhone SE 2022 (v4)", Brand = "Apple", Price = 9990000, ImageUrl = "/images/ProductsApple/iphoneSE2022.jpg", Description = "Giá rẻ - Cấu hình mạnh mẽ trong thân xác nhỏ" },
        new Product { Id = 11, Name = "Samsung Galaxy S24 Ultra", Brand = "Samsung", Price = 27990000, ImageUrl = "/images/ProductsSamsung/SamsungS24Ultra.jpg", Description = "Cao cấp - Galaxy AI quyền năng thống trị", SourceUrl = "https://fptshop.com.vn/dien-thoai/samsung-galaxy-s24-ultra" },
        new Product { Id = 12, Name = "Samsung Galaxy Z Fold6", Brand = "Samsung", Price = 41990000, ImageUrl = "/images/ProductsSamsung/SamsungZfold6.jpg", Description = "Cao cấp - Đỉnh cao công nghệ màn hình gập" },
        new Product { Id = 13, Name = "Samsung Galaxy Z Flip6", Brand = "Samsung", Price = 26990000, ImageUrl = "/images/ProductsSamsung/SamsungZflip6.jpg", Description = "Cao cấp - Gập vỏ sò thời trang tinh tế" },
        new Product { Id = 14, Name = "Samsung Galaxy S24 Plus", Brand = "Samsung", Price = 21990000, ImageUrl = "/images/ProductsSamsung/SamsungS24+.jpg", Description = "Cao cấp - Màn hình 2K mượt mà siêu nét" },
        new Product { Id = 15, Name = "Samsung Galaxy S24 256GB", Brand = "Samsung", Price = 18990000, ImageUrl = "/images/ProductsSamsung/SamsungS24.jpg", Description = "Cao cấp - Flagship nhỏ gọn tích hợp AI" },
        new Product { Id = 16, Name = "Samsung Galaxy S23 FE", Brand = "Samsung", Price = 12490000, ImageUrl = "/images/ProductsSamsung/SamsungS23FE.jpg", Description = "Tầm trung - Cấu hình Flagship giá tiếp cận" },
        new Product { Id = 17, Name = "Samsung Galaxy A55 5G", Brand = "Samsung", Price = 9690000, ImageUrl = "/images/ProductsSamsung/SamsungA55-5G.jpg", Description = "Tầm trung - Khung nhôm cao cấp kháng nước tốt", SourceUrl = "https://fptshop.com.vn/dien-thoai/samsung-galaxy-a55" },
        new Product { Id = 18, Name = "Samsung Galaxy A35 5G", Brand = "Samsung", Price = 7690000, ImageUrl = "/images/ProductsSamsung/SamsungA35-5G.jpg", Description = "Tầm trung - Thiết kế hiện đại pin siêu trâu", SourceUrl = "https://fptshop.com.vn/dien-thoai/samsung-galaxy-a35" },
        new Product { Id = 19, Name = "Samsung Galaxy A15 128GB", Brand = "Samsung", Price = 4290000, ImageUrl = "/images/ProductsSamsung/SamsungA15.jpg", Description = "Giá rẻ - Màn hình Super AMOLED cực mượt", SourceUrl = "https://fptshop.com.vn/dien-thoai/samsung-galaxy-a15" },
        new Product { Id = 20, Name = "Samsung Galaxy A05s", Brand = "Samsung", Price = 3290000, ImageUrl = "/images/ProductsSamsung/SamsungA05s.jpg", Description = "Giá rẻ - Màn hình lớn pin khỏe sạc nhanh", SourceUrl = "https://fptshop.com.vn/dien-thoai/samsung-galaxy-a05s-4gb-128gb" },
        new Product { Id = 21, Name = "Xiaomi 14 Ultra 512GB", Brand = "Xiaomi", Price = 29990000, ImageUrl = "/images/ProductsXiaomi/xiaomi14ultra.jpg", Description = "Cao cấp - Ống kính Leica nâng tầm nhiếp ảnh", SourceUrl = "https://fptshop.com.vn/dien-thoai/xiaomi-14-ultra" },
        new Product { Id = 22, Name = "Xiaomi 14 256GB", Brand = "Xiaomi", Price = 19990000, ImageUrl = "/images/ProductsXiaomi/xiaomi14.jpg", Description = "Cao cấp - Cấu hình hủy diệt phân khúc nhỏ gọn", SourceUrl = "https://fptshop.com.vn/dien-thoai/xiaomi-14" },
        new Product { Id = 23, Name = "Xiaomi 13T Pro 512GB", Brand = "Xiaomi", Price = 15490000, ImageUrl = "/images/ProductsXiaomi/xiaomi13tpro.jpg", Description = "Cao cấp - Sạc siêu tốc 120W màn hình 144Hz" },
        new Product { Id = 24, Name = "Xiaomi Poco X6 Pro 5G", Brand = "Xiaomi", Price = 8990000, ImageUrl = "/images/ProductsXiaomi/pocox6pro-5G.jpg", Description = "Tầm trung - Quái vật cấu hình cày game mượt" },
        new Product { Id = 25, Name = "Xiaomi Redmi Note 13 Pro Plus", Brand = "Xiaomi", Price = 10290000, ImageUrl = "/images/ProductsXiaomi/redminote13pro+.jpg", Description = "Tầm trung - Màn hình cong, camera 200MP", SourceUrl = "https://fptshop.com.vn/dien-thoai/xiaomi-redmi-note-13-pro-plus" },
        new Product { Id = 26, Name = "Xiaomi Redmi Note 13 Pro 5G", Brand = "Xiaomi", Price = 8990000, ImageUrl = "/images/ProductsXiaomi/redminote13pro-5G.jpg", Description = "Tầm trung - Chip Snapdragon chụp ảnh nét căng", SourceUrl = "https://fptshop.com.vn/dien-thoai/xiaomi-redmi-note-13-pro-5g" },
        new Product { Id = 27, Name = "Xiaomi Redmi Note 13 128GB", Brand = "Xiaomi", Price = 4590000, ImageUrl = "/images/ProductsXiaomi/redminote13.jpg", Description = "Giá rẻ - Điện thoại quốc dân màn hình viền mỏng", SourceUrl = "https://fptshop.com.vn/dien-thoai/xiaomi-redmi-note-13" },
        new Product { Id = 28, Name = "Xiaomi Redmi 13 128GB", Brand = "Xiaomi", Price = 4290000, ImageUrl = "/images/ProductsXiaomi/redmi13.jpg", Description = "Giá rẻ - Thiết kế mặt lưng kính sang trọng", SourceUrl = "https://fptshop.com.vn/dien-thoai/xiaomi-redmi-13" },
        new Product { Id = 29, Name = "Xiaomi Redmi A3 64GB", Brand = "Xiaomi", Price = 2490000, ImageUrl = "/images/ProductsXiaomi/redmia3.jpg", Description = "Giá rẻ - Cụm camera tròn độc đáo giá học sinh", SourceUrl = "https://fptshop.com.vn/dien-thoai/xiaomi-redmi-a3" },
        new Product { Id = 30, Name = "Xiaomi Poco M6 128GB", Brand = "Xiaomi", Price = 3890000, ImageUrl = "/images/ProductsXiaomi/pocoM6.jpg", Description = "Giá rẻ - Thiết kế bắt mắt cấu hình ổn định" },
        new Product { Id = 31, Name = "Oppo Find N3 5G", Brand = "Oppo", Price = 44990000, ImageUrl = "/images/ProductsOppo/OppoFindN3-5G.jpg", Description = "Cao cấp - Đỉnh cao gập mỏng nhẹ doanh nhân" },
        new Product { Id = 32, Name = "Oppo Find N3 Flip", Brand = "Oppo", Price = 19990000, ImageUrl = "/images/ProductsOppo/OppoFindN3Flip.jpg", Description = "Cao cấp - Gập màn hình dọc camera cao cấp", SourceUrl = "https://fptshop.com.vn/dien-thoai/oppo-find-n3-flip" },
        new Product { Id = 33, Name = "Oppo Reno12 Pro 5G", Brand = "Oppo", Price = 17990000, ImageUrl = "/images/ProductsOppo/OppoReno12Pro-5G.jpg", Description = "Cao cấp - Thiết kế chuyên gia nhiếp ảnh AI", SourceUrl = "https://fptshop.com.vn/dien-thoai/oppo-reno12-pro" },
        new Product { Id = 34, Name = "Oppo Reno12 5G", Brand = "Oppo", Price = 12990000, ImageUrl = "/images/ProductsOppo/OppoReno12-5G.jpg", Description = "Tầm trung - Smartphone AI tương lai sành điệu", SourceUrl = "https://fptshop.com.vn/dien-thoai/oppo-reno12" },
        new Product { Id = 35, Name = "Oppo Reno11 F 5G", Brand = "Oppo", Price = 8490000, ImageUrl = "/images/ProductsOppo/OppoReno11F-5G.jpg", Description = "Tầm trung - Mặt lưng họa tiết vân kim cương", SourceUrl = "https://fptshop.com.vn/dien-thoai/oppo-reno11-f" },
        new Product { Id = 36, Name = "Oppo A98 5G", Brand = "Oppo", Price = 6990000, ImageUrl = "/images/ProductsOppo/OppoA98-5G.jpg", Description = "Tầm trung - Sạc siêu nhanh màn hình siêu mượt" },
        new Product { Id = 37, Name = "Oppo A79 5G", Brand = "Oppo", Price = 7190000, ImageUrl = "/images/ProductsOppo/OppoA79-5G.jpg", Description = "Tầm trung - Âm thanh loa kép cực lớn bùng nổ", SourceUrl = "https://fptshop.com.vn/dien-thoai/oppo-a79-5g" },
        new Product { Id = 38, Name = "Oppo A60 128GB", Brand = "Oppo", Price = 5490000, ImageUrl = "/images/ProductsOppo/OppoA60.jpg", Description = "Giá rẻ - Màn hình siêu sáng chuẩn độ bền quân đội", SourceUrl = "https://fptshop.com.vn/dien-thoai/oppo-a60-8gb-128gb" },
        new Product { Id = 39, Name = "Oppo A3 128GB", Brand = "Oppo", Price = 4990000, ImageUrl = "/images/ProductsOppo/OppoA3.jpg", Description = "Giá rẻ - Thiết kế bóng bẩy chống va đập cực tốt", SourceUrl = "https://fptshop.com.vn/dien-thoai/oppo-a3" },
        new Product { Id = 40, Name = "Oppo A18 64GB", Brand = "Oppo", Price = 3290000, ImageUrl = "/images/ProductsOppo/OppoA18.jpg", Description = "Giá rẻ - Hoạt động mượt mà lưu trữ thông minh", SourceUrl = "https://fptshop.com.vn/dien-thoai/oppo-a18" },
        new Product { Id = 41, Name = "Vivo X100 Pro 5G", Brand = "Vivo", Price = 24990000, ImageUrl = "/images/ProductsVivo/VivoX100Pro-5G.jpg", Description = "Cao cấp - Ống kính Zeiss đỉnh cao bắt trọn khoảnh khắc" },
        new Product { Id = 42, Name = "Vivo V40 Pro 5G", Brand = "Vivo", Price = 16500000, ImageUrl = "/images/ProductsVivo/VivoV40Pro-5G.jpg", Description = "Cao cấp - Vòng sáng Aura độc quyền chụp ảnh đêm nghệ thuật" },
        new Product { Id = 43, Name = "Vivo V30 5G", Brand = "Vivo", Price = 12490000, ImageUrl = "/images/ProductsVivo/VivoV30-5G.jpg", Description = "Tầm trung - Thiết kế siêu mỏng màn hình cong cuốn hút" },
        new Product { Id = 44, Name = "Vivo V30e 5G", Brand = "Vivo", Price = 9490000, ImageUrl = "/images/ProductsVivo/VivoV30e-5G.jpg", Description = "Tầm trung - Pin lớn thiết kế vân lông vũ tinh tế", SourceUrl = "https://fptshop.com.vn/dien-thoai/vivo-v30e-8gb-128gb" },
        new Product { Id = 45, Name = "Vivo Y100 128GB", Brand = "Vivo", Price = 6990000, ImageUrl = "/images/ProductsVivo/VivoY100.jpg", Description = "Tầm trung - Sạc nhanh 80W phân khúc tầm trung", SourceUrl = "https://fptshop.com.vn/dien-thoai/vivo-y100-8gb-128gb" },
        new Product { Id = 46, Name = "Vivo Y36 128GB", Brand = "Vivo", Price = 5290000, ImageUrl = "/images/ProductsVivo/VivoY36.jpg", Description = "Tầm trung - Thiết kế mặt lưng kính chuyển màu thời thượng" },
        new Product { Id = 47, Name = "Vivo Y28 128GB", Brand = "Vivo", Price = 5490000, ImageUrl = "/images/ProductsVivo/VivoY28.jpg", Description = "Giá rẻ - Loa kép 300% âm lượng pin trâu 6000mAh", SourceUrl = "https://fptshop.com.vn/dien-thoai/vivo-y28-8gb-128gb" },
        new Product { Id = 48, Name = "Vivo Y18 128GB", Brand = "Vivo", Price = 4190000, ImageUrl = "/images/ProductsVivo/VivoY18.jpg", Description = "Giá rẻ - Thiết kế vuông màn hình 90Hz mượt mà", SourceUrl = "https://fptshop.com.vn/dien-thoai/vivo-y18-6gb-128gb" },
        new Product { Id = 49, Name = "Vivo Y18s 128GB", Brand = "Vivo", Price = 4490000, ImageUrl = "/images/ProductsVivo/VivoY18s.jpg", Description = "Giá rẻ - Bộ nhớ lớn camera sắc nét tầm giá", SourceUrl = "https://fptshop.com.vn/dien-thoai/vivo-y18s" },
        new Product { Id = 50, Name = "Vivo Y03 64GB", Brand = "Vivo", Price = 2790000, ImageUrl = "/images/ProductsVivo/VivoY03.jpg", Description = "Giá rẻ - Điện thoại cơ bản siêu bền chống nước nhẹ", SourceUrl = "https://fptshop.com.vn/dien-thoai/vivo-y03" },
        new Product { Id = 51, Name = "Realme GT 6 5G", Brand = "Realme", Price = 16990000, ImageUrl = "/images/ProductsRealme/RealmeGT6-5G.jpg", Description = "Cao cấp - Sát thủ cấu hình màn hình siêu sáng" },
        new Product { Id = 52, Name = "Realme 12 Pro Plus 5G", Brand = "Realme", Price = 11990000, ImageUrl = "/images/ProductsRealme/Realme12ProPlus-5G.jpg", Description = "Cao cấp - Thiết kế đồng hồ xa xỉ camera tiềm vọng" },
        new Product { Id = 53, Name = "Realme 11 Pro 5G", Brand = "Realme", Price = 9290000, ImageUrl = "/images/ProductsRealme/Realme11Pro-5G.jpg", Description = "Tầm trung - Mặt lưng da sinh học sang trọng cuốn hút" },
        new Product { Id = 54, Name = "Realme 12 5G", Brand = "Realme", Price = 7690000, ImageUrl = "/images/ProductsRealme/Realme12-5G.jpg", Description = "Tầm trung - Camera 108MP chi tiết cực cao" },
        new Product { Id = 55, Name = "Realme C67 128GB", Brand = "Realme", Price = 4990000, ImageUrl = "/images/ProductsRealme/RealmeC67.jpg", Description = "Tầm trung - Loa kép stereo chip Snapdragon ổn định", SourceUrl = "https://fptshop.com.vn/dien-thoai/realme-c67" },
        new Product { Id = 56, Name = "Realme C65 128GB", Brand = "Realme", Price = 3990000, ImageUrl = "/images/ProductsRealme/RealmeC65.jpg", Description = "Giá rẻ - Thiết kế vân sao rơi mỏng nhẹ ấn tượng", SourceUrl = "https://fptshop.com.vn/dien-thoai/realme-c65-6gb-128gb" },
        new Product { Id = 57, Name = "Realme C55 128GB", Brand = "Realme", Price = 4190000, ImageUrl = "/images/ProductsRealme/RealmeC55.jpg", Description = "Giá rẻ - Tính năng thông báo mini capsule độc đáo" },
        new Product { Id = 58, Name = "Realme C53 128GB", Brand = "Realme", Price = 3490000, ImageUrl = "/images/ProductsRealme/RealmeC53.jpg", Description = "Giá rẻ - Thiết kế giống iPhone sạc nhanh vượt trội" },
        new Product { Id = 59, Name = "Realme C60 64GB", Brand = "Realme", Price = 2790000, ImageUrl = "/images/ProductsRealme/RealmeC60.jpg", Description = "Giá rẻ - Trải nghiệm bền bỉ màn hình mượt mà", SourceUrl = "https://fptshop.com.vn/dien-thoai/realme-c60" },
        new Product { Id = 60, Name = "Realme Note 50 64GB", Brand = "Realme", Price = 2490000, ImageUrl = "/images/ProductsRealme/RealmeNote50.jpg", Description = "Giá rẻ - Smartphone phá giá phân khúc bình dân", SourceUrl = "https://fptshop.com.vn/dien-thoai/realme-note-50" },
        new Product { Id = 61, Name = "OnePlus 12 5G", Brand = "OnePlus", Price = 21990000, ImageUrl = "/images/ProductsOnePlus/OnePlus12-5G.jpg", Description = "Cao cấp - Flagship hoàn hảo mượt mà không độ trễ" },
        new Product { Id = 62, Name = "OnePlus 12R 5G", Brand = "OnePlus", Price = 14990000, ImageUrl = "/images/ProductsOnePlus/OnePlus12R-5G.jpg", Description = "Cao cấp - Sát thủ gaming pin trâu tản nhiệt khủng" },
        new Product { Id = 63, Name = "OnePlus Open", Brand = "OnePlus", Price = 39990000, ImageUrl = "/images/ProductsOnePlus/OnePlusOpen.jpg", Description = "Cao cấp - Đột phá màn hình gập không nếp gấp" },
        new Product { Id = 64, Name = "OnePlus Nord 4 5G", Brand = "OnePlus", Price = 11490000, ImageUrl = "/images/ProductsOnePlus/OnePlusNord4-5G.jpg", Description = "Tầm trung - Vỏ kim loại nguyên khối độc nhất vô nhị" },
        new Product { Id = 65, Name = "OnePlus Nord CE4", Brand = "OnePlus", Price = 8490000, ImageUrl = "/images/ProductsOnePlus/OnePlusNordCE4.jpg", Description = "Tầm trung - Sạc nhanh 100W phân khúc tầm trung" },
        new Product { Id = 66, Name = "OnePlus Nord 3 5G", Brand = "OnePlus", Price = 9990000, ImageUrl = "/images/ProductsOnePlus/OnePlusNord35G.jpg", Description = "Tầm trung - Hiệu năng cực mạnh màn hình viền siêu mỏng" },
        new Product { Id = 67, Name = "OnePlus Nord CE 3 Lite 5G", Brand = "OnePlus", Price = 5990000, ImageUrl = "/images/ProductsOnePlus/OnePlusNordCE3Lite-5G.jpg", Description = "Giá rẻ - Camera 108MP thiết kế trẻ trung cá tính" },
        new Product { Id = 68, Name = "OnePlus Nord N30 SE 5G", Brand = "OnePlus", Price = 4500000, ImageUrl = "/images/ProductsOnePLUS/OnePLUSNordN30SE-5G.jpg", Description = "Giá rẻ - Hỗ trạng mạng 5G sạc nhanh phân khúc bình dân" },
        new Product { Id = 69, Name = "OnePLUS Nord N20 SE", Brand = "OnePlus", Price = 3190000, ImageUrl = "/images/ProductsOnePLUS/OnePLUSNordN20SE.jpg", Description = "Giá rẻ - Loa kép âm thanh sống động giá tốt" },
        new Product { Id = 70, Name = "OnePLUS Nord N100", Brand = "OnePlus", Price = 2590000, ImageUrl = "/images/ProductsOnePLUS/OnePLUSNordN100.jpg", Description = "Giá rẻ - Pin trâu 5000mAh trải nghiệm bền bỉ cả ngày" },
        new Product { Id = 71, Name = "Honor Magic6 Pro 5G", Brand = "Honor", Price = 23990000, ImageUrl = "/images/ProductsHonor/HonorMagic6Pro-5G.jpg", Description = "Cao cấp - Camera zoom siêu nét dung lượng pin khủng" },
        new Product { Id = 72, Name = "Honor 200 Pro 5G", Brand = "Honor", Price = 18490000, ImageUrl = "/images/ProductsHonor/Honor200Pro-5G.jpg", Description = "Cao cấp - Chuyên gia chụp chân dung nghệ thuật Studio Harcourt" },
        new Product { Id = 73, Name = "Honor 200 5G", Brand = "Honor", Price = 12990000, ImageUrl = "/images/ProductsHonor/Honor200-5G.jpg", Description = "Tầm trung - Màn hình bảo vệ mắt đỉnh cao công nghệ" },
        new Product { Id = 74, Name = "Honor 90 5G", Brand = "Honor", Price = 9490000, ImageUrl = "/images/ProductsHonor/Honor90-5G.jpg", Description = "Tầm trung - Thiết kế mỏng camera độ phân giải siêu khủng" },
        new Product { Id = 75, Name = "Honor X9b 5G", Brand = "Honor", Price = 8490000, ImageUrl = "/images/ProductsHonor/HonorX9b-5G.jpg", Description = "Tầm trung - Màn hình chống vỡ chuẩn 5 sao siêu bền bỉ", SourceUrl = "https://fptshop.com.vn/dien-thoai/honor-x9b" },
        new Product { Id = 76, Name = "Honor X8b 256GB", Brand = "Honor", Price = 5990000, ImageUrl = "/images/ProductsHonor/HonorX8b.jpg", Description = "Giá rẻ - Bộ nhớ khủng thiết kế viên thuốc độc đáo", SourceUrl = "https://fptshop.com.vn/dien-thoai/honor-x8b" },
        new Product { Id = 77, Name = "Honor X7b 128GB", Brand = "Honor", Price = 4790000, ImageUrl = "/images/ProductsHonor/HonorX7b.jpg", Description = "Giá rẻ - Pin trâu 6000mAh loa kép cực đại cuốn hút", SourceUrl = "https://fptshop.com.vn/dien-thoai/honor-x7b" },
        new Product { Id = 78, Name = "Honor X6a 128GB", Brand = "Honor", Price = 3290000, ImageUrl = "/images/ProductsHonor/HonorX6a.jpg", Description = "Giá rẻ - Màn hình mượt mà bộ nhớ lưu trữ thoải mái", SourceUrl = "https://fptshop.com.vn/dien-thoai/honor-x6a" },
        new Product { Id = 79, Name = "Honor X5 Plus", Brand = "Honor", Price = 2490000, ImageUrl = "/images/ProductsHonor/HonorX5Plus.jpg", Description = "Giá rẻ - Pin khỏe màn hình lớn tầm giá sinh viên", SourceUrl = "https://fptshop.com.vn/dien-thoai/honor-x5-plus" },
        new Product { Id = 80, Name = "Honor 90 Lite", Brand = "Honor", Price = 5190000, ImageUrl = "/images/ProductsHonor/Honor90Lite.jpg", Description = "Giá rẻ - Thiết kế viền mỏng trải nghiệm tối ưu" },
        new Product { Id = 81, Name = "Tecno Phantom V Fold", Brand = "Tecno", Price = 23990000, ImageUrl = "/images/ProductsTecno/TecnoPhantomVFold.jpg", Description = "Cao cấp - Flagship màn hình gập giá tốt nhất thị trường" },
        new Product { Id = 82, Name = "Tecno Camon 30 Pro 5G", Brand = "Tecno", Price = 10990000, ImageUrl = "/images/ProductsTecno/TecnoCamon30Pro-5G.jpg", Description = "Cao cấp - Camera Sony chuyên nghiệp chống rung OIS" },
        new Product { Id = 83, Name = "Tecno Pova 6 Pro 5G", Brand = "Tecno", Price = 7490000, ImageUrl = "/images/ProductsTecno/TecnoPova6Pro-5G.jpg", Description = "Tầm trung - Thiết kế mặt lưng LED Mecha đậm chất gaming" },
        new Product { Id = 84, Name = "Tecno Camon 30 5G", Brand = "Tecno", Price = 6490000, ImageUrl = "/images/ProductsTecno/TecnoCamon30-5G.jpg", Description = "Tầm trung - Mặt lưng da sinh học thiết kế cổ điển độc lạ" },
        new Product { Id = 85, Name = "Tecno Pova 5", Brand = "Tecno", Price = 4290000, ImageUrl = "/images/ProductsTecno/TecnoPova5.jpg", Description = "Tầm trung - Pin trâu sạc nhanh quái vật phân khúc máy phụ" },
        new Product { Id = 86, Name = "Tecno Spark 20 Pro Plus", Brand = "Tecno", Price = 5290000, ImageUrl = "/images/ProductsTecno/Tecno Spark 20 Pro Plus.jpg", Description = "Giá rẻ - Màn hình cong AMOLED viền mỏng vô cực", SourceUrl = "https://fptshop.com.vn/dien-thoai/tecno-spark-20-pro-plus" },
        new Product { Id = 87, Name = "Tecno Spark 20 Pro", Brand = "Tecno", Price = 4290000, ImageUrl = "/images/ProductsTecno/Tecno Spark 20 Pro.jpg", Description = "Giá rẻ - Cấu hình mạnh mẽ thiết kế camera sang chảnh", SourceUrl = "https://fptshop.com.vn/dien-thoai/tecno-spark-20-pro" },
        new Product { Id = 88, Name = "Tecno Spark 20 128GB", Brand = "Tecno", Price = 3290000, ImageUrl = "/images/ProductsTecno/Tecno Spark 20.jpg", Description = "Giá rẻ - Loa kép âm thanh nổi bộ nhớ thoải mái", SourceUrl = "https://fptshop.com.vn/dien-thoai/tecno-spark-20" },
        new Product { Id = 89, Name = "Tecno Spark 20C", Brand = "Tecno", Price = 2590000, ImageUrl = "/images/ProductsTecno/Tecno Spark 20C.jpg ", Description = "Giá rẻ - Smartphone đa nhiệm mượt mà giá siêu sinh viên", SourceUrl = "https://fptshop.com.vn/dien-thoai/tecno-spark-20c" },
        new Product { Id = 90, Name = "Tecno Spark Go 2024", Brand = "Tecno", Price = 1890000, ImageUrl = "/images/ProductsTecno/Tecno Spark Go 2024.jpg", Description = "Giá rẻ - Điện thoại thông minh phá giá rẻ nhất hiện nay", SourceUrl = "https://fptshop.com.vn/dien-thoai/tecno-spark-go-2024" },
        new Product { Id = 91, Name = "Infinix GT 20 Pro 5G", Brand = "Infinix", Price = 9990000, ImageUrl = "/images/ProductsInfinix/Infinix GT 20 Pro 5G.jpg", Description = "Cao cấp - Điện thoại thi đấu Esport chuyên dụng mạnh mẽ" },
        new Product { Id = 92, Name = "Infinix Note 40 Pro 5G", Brand = "Infinix", Price = 7990000, ImageUrl = "/images/ProductsInfinix/Infinix Note 40 Pro 5G.jpg", Description = "Cao cấp - Sạc nhanh không dây từ tính cao cấp độc lạ" },
        new Product { Id = 93, Name = "Infinix Note 40 256GB", Brand = "Infinix", Price = 5490000, ImageUrl = "/images/ProductsInfinix/Infinix Note 40.jpg", Description = "Tầm trung - Màn hình AMOLED viền siêu mỏng sạc thần tốc" },
        new Product { Id = 94, Name = "Infinix Note 30 5G", Brand = "Infinix", Price = 4990000, ImageUrl = "/images/ProductsInfinix/Infinix Note 30 5G.jpg", Description = "Tầm trung - Kết nối 5G mượt mà tối ưu trải nghiệm mạng" },
        new Product { Id = 95, Name = "Infinix Hot 40 Pro", Brand = "Infinix", Price = 4290000, ImageUrl = "/images/ProductsInfinix/Infinix Hot 40 Pro.jpg", Description = "Tầm trung - Chip tối ưu chơi game mượt phân khúc máy phụ" },
        new Product { Id = 96, Name = "Infinix Hot 40i 256GB", Brand = "Infinix", Price = 3190000, ImageUrl = "/images/ProductsInfinix/Infinix Hot 40i.jpg", Description = "Giá rẻ - Bộ nhớ siêu khủng lưu trữ thả ga không giới hạn" },
        new Product { Id = 97, Name = "Infinix Hot 30i 128GB", Brand = "Infinix", Price = 2590000, ImageUrl = "/images/ProductsInfinix/Infinix Hot 30i.jpg", Description = "Giá rẻ - Thiết kế mặt lưng kim cương bắt mắt cá tính" },
        new Product { Id = 98, Name = "Infinix Smart 8 64GB", Brand = "Infinix", Price = 1890000, ImageUrl = "/images/ProductsInfinix/Infinix Smart 8.jpg", Description = "Giá rẻ - Đèn thông báo trạng thái thông minh quanh camera" },
        new Product { Id = 99, Name = "Infinix Smart 7", Brand = "Infinix", Price = 1690000, ImageUrl = "/images/ProductsInfinix/Infinix Smart 7.jpg", Description = "Giá rẻ - Kháng khuẩn mặt lưng vỏ máy bền bỉ an toàn" },
        new Product { Id = 100, Name = "Infinix Zero 30 5G", Brand = "Infinix", Price = 8490000, ImageUrl = "/images/ProductsInfinix/Infinix Zero 30 5G.jpg", Description = "Giá rẻ - Camera quay phim chất lượng cao vlog chuyên nghiệp" },
        new Product { Id = 101, Name = "Google Pixel 9 Pro XL", Brand = "Google Pixel", Price = 28990000, ImageUrl = "/images/ProductsGooglePixel/Google Pixel 9 Pro XL.jpg", Description = "Cao cấp - Trí tuệ nhân tạo thuần khiết Google đỉnh cao" },
        new Product { Id = 102, Name = "Google Pixel 9 Pro", Brand = "Google Pixel", Price = 25500000, ImageUrl = "/images/ProductsGooglePixel/Google Pixel 9 Pro.jpg", Description = "Cao cấp - Camera thuật toán xuất sắc chân thực sống động" },
        new Product { Id = 103, Name = "Google Pixel 8 Pro", Brand = "Google Pixel", Price = 18500000, ImageUrl = "/images/ProductsGooglePixel/Google Pixel 8 Pro.jpg", Description = "Cao cấp - Màn hình siêu sáng mượt mà thiết kế cao cấp" },
        new Product { Id = 104, Name = "Google Pixel 9", Brand = "Google Pixel", Price = 21000000, ImageUrl = "/images/ProductsGooglePixel/Google Pixel 9.png", Description = "Tầm trung - Trải nghiệm mượt mà sang trọng nhỏ gọn thời trang" },
        new Product { Id = 105, Name = "Google Pixel 8a", Brand = "Google Pixel", Price = 12990000, ImageUrl = "/images/ProductsGooglePixel/Google Pixel 8a.jpg", Description = "Tầm trung - Phiên bản rút gọn cấu hình khủng từ Google" },
        new Product { Id = 106, Name = "Google Pixel 7 Pro", Brand = "Google Pixel", Price = 13500000, ImageUrl = "/images/ProductsGooglePixel/Google Pixel 7 Pro.jpg", Description = "Tầm trung - Cựu vương nhiếp ảnh thiết kế độc đáo ấn tượng" },
        new Product { Id = 107, Name = "Google Pixel 7a", Brand = "Google Pixel", Price = 9500000, ImageUrl = "/images/ProductsGooglePixel/Google Pixel 7a.jpg", Description = "Giá rẻ - Trải nghiệm Android thuần khiết mượt mà lâu dài" },
        new Product { Id = 108, Name = "Google Pixel 6 Pro", Brand = "Google Pixel", Price = 8200000, ImageUrl = "/images/ProductsGooglePixel/Google Pixel 6 Pro.jpg", Description = "Giá rẻ - Khởi đầu chip Google Tensor xử lý thông minh" },
        new Product { Id = 109, Name = "Google Pixel 6a", Brand = "Google Pixel", Price = 5900000, ImageUrl = "/images/ProductsGooglePixel/Google Pixel 6a.jpg", Description = "Giá rẻ - Máy phụ chụp ảnh sắc nét bền bỉ phân khúc" },
        new Product { Id = 110, Name = "Google Pixel 5 5G", Brand = "Google Pixel", Price = 3900000, ImageUrl = "/images/ProductsGooglePixel/Google Pixel 5 5G.jpg", Description = "Giá rẻ - Thiết kế kim loại nhỏ gọn huyền thoại siêu bền" },
        new Product { Id = 111, Name = "Asus ROG Phone 8 Pro", Brand = "Asus Rog Phone", Price = 28990000, ImageUrl = "/images/ProductsROG/Asus ROG Phone 8 Pro.jpg", Description = "Cao cấp - Nhà vua gaming màn hình 165Hz thiết kế mỏng" },
        new Product { Id = 112, Name = "Asus ROG Phone 8", Brand = "Asus Rog Phone", Price = 23990000, ImageUrl = "/images/ProductsROG/Asus ROG Phone 8.jpg", Description = "Cao cấp - Đẳng cấp game thủ hỗ trợ kháng nước cao cấp" },
        new Product { Id = 113, Name = "Asus ROG Phone 7 Ultimate", Brand = "Asus Rog Phone", Price = 27500000, ImageUrl = "/images/ProductsROG/Asus ROG Phone 7 Ultimate.jpg", Description = "Cao cấp - Mặt lưng có khe mở tản nhiệt khí độc lạ" },
        new Product { Id = 114, Name = "Asus ROG Phone 7", Brand = "Asus Rog Phone", Price = 18990000, ImageUrl = "/images/ProductsROG/Asus ROG Phone 7.jpg", Description = "Tầm trung - Pin khủng 6000mAh loa ngoài hay nhất thế giới" },
        new Product { Id = 115, Name = "Asus ROG Phone 6 Pro", Brand = "Asus Rog Phone", Price = 15500000, ImageUrl = "/images/ProductsROG/Asus ROG Phone 6 Pro.jpg", Description = "Tầm trung - Màn hình phụ OLED mặt sau hiển thị cá tính" },
        new Product { Id = 116, Name = "Asus ROG Phone 6", Brand = "Asus Rog Phone", Price = 12900000, ImageUrl = "/images/ProductsROG/Asus ROG Phone 6.jpg", Description = "Tầm trung - Chip Snapdragon tối ưu mượt cày game xuyên đêm" },
        new Product { Id = 117, Name = "Asus ROG Phone 5s Pro", Brand = "Asus Rog Phone", Price = 11500000, ImageUrl = "/images/ProductsROG/Asus ROG Phone 5s Pro.jpg", Description = "Giá rẻ - Thiết kế hầm hố tích hợp cảm biến trigger nhạy" },
        new Product { Id = 118, Name = "Asus ROG Phone 5", Brand = "Asus Rog Phone", Price = 8900000, ImageUrl = "/images/ProductsROG/Asus ROG Phone 5.jpg", Description = "Giá rẻ - Trải nghiệm gaming đỉnh cao mức giá cực tốt" },
        new Product { Id = 119, Name = "Asus ROG Phone 3", Brand = "Asus Rog Phone", Price = 5500000, ImageUrl = "/images/ProductsROG/Asus ROG Phone 3.jpg", Description = "Giá rẻ - Máy phụ cày game treo acc cực bền bỉ ổn định" },
        new Product { Id = 120, Name = "Asus ROG Phone II", Brand = "Asus Rog Phone", Price = 3500000, ImageUrl = "/images/ProductsROG/Asus ROG Phone II.jpg", Description = "Giá rẻ - Huyền thoại màn hình 120Hz đời đầu giá rẻ" },
        new Product { Id = 121, Name = "Nothing Phone (2) 256GB", Brand = "Nothing phone", Price = 16490000, ImageUrl = "/images/ProductsNothing/Nothing Phone (2).jpg", Description = "Cao cấp - Đèn LED Glyph độc bản mặt lưng trong suốt nghệ thuật" },
        new Product { Id = 122, Name = "Nothing Phone (2a) Plus", Brand = "Nothing phone", Price = 11990000, ImageUrl = "/images/ProductsNothing/Nothing Phone (2a) Plus.jpg", Description = "Cao cấp - Phiên bản nâng cấp hiệu năng thiết kế tương lai" },
        new Product { Id = 123, Name = "Nothing Phone (1) 256GB", Brand = "Nothing phone", Price = 9990000, ImageUrl = "/images/ProductsNothing/Nothing Phone (1).jpg", Description = "Tầm trung - Định nghĩa lại thiết kế smartphone phong cách độc lạ" },
        new Product { Id = 124, Name = "Nothing Phone (2a) 128GB", Brand = "Nothing phone", Price = 8490000, ImageUrl = "/images/ProductsNothing/Nothing Phone (2a).jpg", Description = "Tầm trung - Cân bằng hoàn hảo giữa hiệu năng và đèn nghệ thuật" },
        new Product { Id = 125, Name = "Nothing CMF Phone 1", Brand = "Nothing phone", Price = 6200000, ImageUrl = "/images/ProductsNothing/Nothing CMF Phone 1.jpg", Description = "Giá rẻ - Khả năng tháo rời thay thế vỏ ốc vít độc lạ sáng tạo" },
        new Product { Id = 126, Name = "Nothing Phone (1) Rút gọn", Brand = "Nothing phone", Price = 7500000, ImageUrl = "/images/ProductsNothing/Nothing Phone (1) Rút gọn.jpg", Description = "Giá rẻ - Ngoại hình độc đáo mức giá tiếp cận dễ dàng" },
        new Product { Id = 127, Name = "Nothing CMF Phone 1 Lite", Brand = "Nothing phone", Price = 4900000, ImageUrl = "/images/ProductsNothing/Nothing CMF Phone 1 Lite.jpg", Description = "Giá rẻ - Tối giản tinh tế phong cách tối giản Bắc Âu" },
        new Product { Id = 128, Name = "Nothing Phone 2a SE", Brand = "Nothing phone", Price = 8990000, ImageUrl = "/images/ProductsNothing/Nothing Phone 2a SE.jpg", Description = "Tầm trung - Bản giới hạn phối màu sắc sảo nghệ thuật" },
        new Product { Id = 129, Name = "Nothing Phone (1) LikeNew", Brand = "Nothing phone", Price = 5900000, ImageUrl = "/images/ProductsNothing/Nothing Phone (1) LikeNew.jpg", Description = "Giá rẻ - Đèn LED đầy đủ giá siêu tốt mượt mà ổn định" },
        new Product { Id = 130, Name = "Nothing Ear Phone Companion", Brand = "Nothing phone", Price = 3990000, ImageUrl = "/images/ProductsNothing/Nothing Ear Phone Companion.jpg", Description = "Giá rẻ - Bản thiết kế đặc biệt nhỏ gọn cá tính độc đáo" },
        new Product { Id = 131, Name = "iQOO 12 Pro 5G", Brand = "IQOO", Price = 19500000, ImageUrl = "/images/ProductsIQOO/iQOO 12 Pro 5G.jpg", Description = "Cao cấp - Sát thủ tốc độ phiên bản hợp tác BMW cá tính" },
        new Product { Id = 132, Name = "iQOO 12 5G", Brand = "IQOO", Price = 16200000, ImageUrl = "/images/ProductsIQOO/iQOO 12 5G.jpg", Description = "Cao cấp - Chip Snap mạnh mẽ sạc siêu tốc tối ưu gaming" },
        new Product { Id = 133, Name = "iQOO Neo9 Pro", Brand = "IQOO", Price = 12500000, ImageUrl = "/images/ProductsIQOO/iQOO Neo9 Pro.jpg", Description = "Cao cấp - Thiết kế hai màu mặt lưng da trẻ trung phá cách" },
        new Product { Id = 134, Name = "iQOO Neo9 5G", Brand = "IQOO", Price = 9900000, ImageUrl = "/images/ProductsIQOO/iQOO Neo9 5G.jpg", Description = "Tầm trung - Trải nghiệm mượt mà hiệu năng hủy diệt phân khúc" },
        new Product { Id = 135, Name = "iQOO Z9 Turbo", Brand = "IQOO", Price = 7800000, ImageUrl = "/images/ProductsIQOO/iQOO Z9 Turbo.jpg", Description = "Tầm trung - Pin khủng 6000mAh cấu hình cực bá đạo cày game" },
        new Product { Id = 136, Name = "iQOO Z9 5G", Brand = "IQOO", Price = 6200000, ImageUrl = "/images/ProductsIQOO/iQOO Z9 5G.jpg", Description = "Tầm trung - Màn hình AMOLED 144Hz siêu mượt phản hồi nhanh" },
        new Product { Id = 137, Name = "iQOO Z9x 5G", Brand = "IQOO", Price = 4690000, ImageUrl = "/images/ProductsIQOO/iQOO Z9x 5G.jpg", Description = "Giá rẻ - Điện thoại pin trâu giá rẻ cấu hình khỏe re", SourceUrl = "https://fptshop.com.vn/dien-thoai/iqoo-z9x" },
        new Product { Id = 138, Name = "iQOO Z7 Pro", Brand = "IQOO", Price = 6900000, ImageUrl = "/images/ProductsIQOO/iQOO Z7 Pro.jpg", Description = "Giá rẻ - Thiết kế màn cong mỏng nhẹ phân khúc bình dân" },
        new Product { Id = 139, Name = "iQOO Z7x 5G", Brand = "IQOO", Price = 4100000, ImageUrl = "/images/ProductsIQOO/iQOO Z7x 5G.jpg", Description = "Giá rẻ - Sạc siêu nhanh pin bền bỉ trải nghiệm tối ưu" },
        new Product { Id = 140, Name = "iQOO Z10 Turbo Plus", Brand = "IQOO", Price = 2900000, ImageUrl = "/images/ProductsIQOO/iQOO Z10 Turbo Plus.jpg", Description = "Giá rẻ - Smartphone cơ bản mượt mà mức giá sinh viên cực tốt" },
        new Product { Id = 141, Name = "Sony Xperia 1 V", Brand = "Sony", Price = 24990000, ImageUrl = "/images/ProductsSony/Sony Xperia 1 V.jpg", Description = "Cao cấp - Cảm biến hình ảnh cách mạng, Màn hình 4K 21:9 chuẩn điện ảnh" },
        new Product { Id = 142, Name = "Sony Xperia 5 V", Brand = "Sony", Price = 18990000, ImageUrl = "/images/ProductsSony/Sony Xperia 5 V.jpg", Description = "Cao cấp - Nhỏ gọn cao cấp, Camera chuyên nghiệp bỏ túi" },
        new Product { Id = 143, Name = "Sony Xperia 10 V", Brand = "Sony", Price = 8990000, ImageUrl = "/images/ProductsSony/Sony Xperia 10 V.jpg", Description = "Tầm trung - Pin trâu nhẹ nhất thế giới, Âm thanh Hi-Res" },
        new Product { Id = 144, Name = "Nubia Neo 2 Gaming", Brand = "Nubia", Price = 4590000, ImageUrl = "/images/ProductsNubia/Nubia Neo 2 Gaming.jpg", Description = "Giá rẻ - Thiết kế Mecha viễn tưởng, Nút Trigger cảm ứng nhanh" },
        new Product { Id = 145, Name = "Nubia Z60 Ultra", Brand = "Nubia", Price = 17990000, ImageUrl = "/images/ProductsNubia/Nubia Z60 Ultra.jpg", Description = "Cao cấp - Camera under-display toàn diện hàng đầu" },
        new Product { Id = 146, Name = "Nubia Red Magic 9 Pro", Brand = "Nubia", Price = 15990000, ImageUrl = "/images/ProductsNubia/Nubia Red Magic 9 Pro.jpg", Description = "Tầm trung - Quái vật gaming mát lạnh quạt tản nhiệt tích hợp" },
        new Product { Id = 147, Name = "iPhone 14 Plus 128GB", Brand = "Apple", Price = 17990000, ImageUrl = "/images/ProductsApple/Iphone 14 Plus.jpg", Description = "Cao cấp - Màn hình lớn pin trâu bậc nhất iPhone" },
        new Product { Id = 148, Name = "Realme GT 5 Pro", Brand = "Realme", Price = 12490000, ImageUrl = "/images/ProductsRealme/Realme GT 5 Pro.jpg", Description = "Tầm trung - Sát thủ cấu hình Snapdragon, sạc 100W siêu nhanh" },
        new Product { Id = 149, Name = "Samsung Galaxy Z Fold5 5G", Brand = "Samsung", Price = 32990000, ImageUrl = "/images/ProductsSamsung/Samsung Galaxy Z Fold5 5G.jpg", Description = "Cao cấp - Đỉnh cao màn hình gập đa nhiệm thế hệ mới" },
        new Product { Id = 150, Name = "OPPO Find X7 Ultra", Brand = "Oppo", Price = 19500000, ImageUrl = "/images/ProductsOppo/OPPO Find X7 Ultra.jpg", Description = "Cao cấp - Camera tàu ngầm kép ưu đãi đặt mua ngay" },
        new Product { Id = 151, Name = "OPPO Reno11 Pro 5G", Brand = "Oppo", Price = 14990000, ImageUrl = "/images/ProductsOppo/OPPO Reno11 Pro 5G.jpg", Description = "Tầm trung - Chuyên gia chân dung thế hệ mới, mặt lưng dòng chảy" }
    };

    private static readonly Dictionary<int, int> _purchaseCount = new();

    static ProductController()
    {
        foreach (var p in _products)
        {
            _purchaseCount[p.Id] = new Random().Next(50, 500);
        }
    }

    public static List<Product> GetAllProducts() => _products;

    public static int GetPurchaseCount(int productId) =>
        _purchaseCount.TryGetValue(productId, out var c) ? c : 0;

    public static double GetAverageRating(int productId) => 0;
    public static int GetReviewCount(int productId) => 0;

    public async Task<IActionResult> Details(int id)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return NotFound();

        var related = await _db.Products
            .Where(p => p.Brand == product.Brand && p.Id != product.Id)
            .Take(4)
            .ToListAsync();
        ViewBag.RelatedProducts = related;

        var reviews = await _db.Reviews
            .Where(r => r.ProductId == id)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        ViewBag.Reviews = reviews;
        ViewBag.AvgRating = reviews.Count == 0 ? 0 : Math.Round(reviews.Average(r => r.Rating), 1);
        ViewBag.ReviewCount = reviews.Count;

        var reviewerIds = reviews.Where(r => r.ReviewerUserId.HasValue).Select(r => r.ReviewerUserId!.Value).Distinct().ToList();
        var avatars = await _db.Users.Where(u => reviewerIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.AvatarUrl);
        ViewBag.ReviewerAvatars = avatars;

        ViewBag.Specs = ProductSpecs.Generate(product.Brand, product.Name, product.Price);

        return View(product);
    }

    public async Task<IActionResult> Index(string brand, string searchTerm, string priceRange, int page = 1)
    {
        var query = _db.Products.AsQueryable();

        if (!string.IsNullOrEmpty(brand))
            query = query.Where(p => p.Brand == brand);

        if (!string.IsNullOrEmpty(searchTerm))
            query = query.Where(p => p.Name.Contains(searchTerm));

        if (!string.IsNullOrEmpty(priceRange))
        {
            query = priceRange switch
            {
                "under5" => query.Where(p => p.Price < 5000000),
                "5to10" => query.Where(p => p.Price >= 5000000 && p.Price <= 10000000),
                "10to20" => query.Where(p => p.Price > 10000000 && p.Price <= 20000000),
                "over20" => query.Where(p => p.Price > 20000000),
                _ => query
            };
        }

        int pageSize = 12;
        int totalItems = await query.CountAsync();
        int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        if (page < 1) page = 1;
        if (page > totalPages && totalPages > 0) page = totalPages;

        var pagedProducts = await query
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var allBrands = await _db.Products.Select(p => p.Brand).Distinct().ToListAsync();

        // Pre-compute ratings and purchase counts for this page
        var productIds = pagedProducts.Select(p => p.Id).ToList();
        var ratingData = await _db.Reviews
            .Where(r => productIds.Contains(r.ProductId))
            .GroupBy(r => r.ProductId)
            .Select(g => new { ProductId = g.Key, Avg = g.Average(r => (double)r.Rating), Count = g.Count() })
            .ToListAsync();
        ViewBag.AvgRatings = ratingData.ToDictionary(r => r.ProductId, r => r.Avg);
        ViewBag.ReviewCounts = ratingData.ToDictionary(r => r.ProductId, r => r.Count);

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.Brands = allBrands;
        ViewBag.SelectedBrand = brand;
        ViewBag.SearchTerm = searchTerm;
        ViewBag.SelectedPriceRange = priceRange;

        return View(pagedProducts);
    }

    [HttpGet]
    public async Task<JsonResult> GetSuggestions(string term, string brand)
    {
        if (string.IsNullOrEmpty(term)) return Json(new List<object>());

        var query = _db.Products.AsQueryable();

        if (!string.IsNullOrEmpty(brand))
            query = query.Where(p => p.Brand == brand);

        var suggestions = await query
            .Where(p => p.Name.Contains(term))
            .Select(p => new { id = p.Id, name = p.Name, imageUrl = p.ImageUrl })
            .Take(5)
            .ToListAsync();

        return Json(suggestions);
    }

    [HttpPost]
    public async Task<IActionResult> SubmitReview(int productId, int rating, string comment)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == productId);
        if (product is null) return NotFound();

        var userId = User.Identity?.IsAuthenticated == true
            ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            : null;

        var userName = User.Identity?.IsAuthenticated == true
            ? User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Khách Hàng"
            : "Khách Hàng";

        _db.Reviews.Add(new Review
        {
            ProductId = productId,
            ReviewerName = userName,
            ReviewerUserId = userId is not null ? int.Parse(userId) : null,
            Rating = Math.Clamp(rating, 1, 5),
            Comment = comment ?? string.Empty,
            CreatedAt = DateTime.Now
        });

        await _db.SaveChangesAsync();

        TempData["ReviewSuccess"] = "Cảm ơn bạn đã đánh giá sản phẩm!";
        return RedirectToAction("Details", new { id = productId });
    }
}

using HueTour.CoreBusiness;

namespace HueTour.WebApp.Services;

public sealed record TourGalleryPhoto(
    string Url,
    string Caption,
    string? Credit = null,
    string? CreditUrl = null);

public static class TourImageCatalog
{
    private const string DragonBoatImage = "https://upload.wikimedia.org/wikipedia/commons/thumb/e/e1/Dragon_boat_on_the_Perfume_River_%2825606150197%29.jpg/1280px-Dragon_boat_on_the_Perfume_River_%2825606150197%29.jpg";
    private const string BaoVinhImage = "https://upload.wikimedia.org/wikipedia/commons/thumb/b/bd/Bao_Vinh_street.jpg/1280px-Bao_Vinh_street.jpg";
    private const string RuChaImage = "https://hue.gov.vn/Portals/0/Uploads/00.00.H57/Nam2025/Thang6/z5862841503198458c3a50b5d9fa36ebde75e83e601fed_17271732773451054397863_28_4_2025_17_12_01_832_CH.jpg?v=1773705600033";
    private const string ThanhTanHotSpringImage = "https://huetourism.gov.vn/Medias/Nam2023/Thang8/huetourism-suoi-khoang-nong-thanh-tan-5.jpg";
    private const string ThanhTanOnsenImage = "https://huetourism.gov.vn/Medias/Nam2023/Thang8/huetourism-suoi-khoang-nong-thanh-tan-11.jpg";
    private const string ThuyBieuGardenHouseImage = "https://huetourism.gov.vn/Portals/0/visithue.cfoo/35521179c4bf33e16aae.jpg";
    private const string ThuyBieuFruitGardenImage = "https://huetourism.gov.vn/Portals/0/visithue.cfoo/f1581834cdf23aac63e3.jpg";
    private const string ThuyXuanIncenseImage = "https://huetourism.gov.vn/Medias/Nam2022/Thang8/huetourism-lang-huong-tram-tuoi-lam-du-lich-1.jpg";
    private const string ThuyTienFlowersImage = "https://upload.wikimedia.org/wikipedia/commons/4/47/Hoa_giay_Thanh_Tien.JPG";
    private const string GiaLongTombImage = "https://thumb.wikimedia.org/wikipedia/commons/thumb/a/a1/Hue_Vietnam_Tomb-of-Emperor-Gia-Long-07.jpg/1280px-Hue_Vietnam_Tomb-of-Emperor-Gia-Long-07.jpg";
    private const string VongCanhImage = "https://huetourism.gov.vn/Medias/Nam2025/Thang7/huetourism-len-doi-vong-canh-ngam-tron-hue-mong-mo-giua-troi-xanh-song-biec-3.jpg";
    private const string AnHienHouseImage = "https://huetourism.gov.vn/Medias/Nam2023/Thang11/huetourism-nha-vuon-an-hien-binh-lang-cua-xu-hue-%F0%9F%8D%80%F0%9F%8D%80-1.jpg";
    private const string AnHienGardenImage = "https://huetourism.gov.vn/Medias/Nam2023/Thang11/huetourism-nha-vuon-an-hien-binh-lang-cua-xu-hue-%F0%9F%8D%80%F0%9F%8D%80-3.jpg";
    private const string VancuBunImage = "https://huetourism.gov.vn/Medias/Nam2025/Thang5/huetourism-an-tuong-chuong-trinh-quang-dien-%E2%80%9Cvan-cu-tinh-hoa-nghe-bun%E2%80%9D-3.jpg";
    private const string HonChaoImage = "https://huetourism.gov.vn/Portals/0/VisitHue/Destination/CanhQuan/visithue_dao%20Hon%20Chao.jpg";
    private const string KimLongCommunalHouseImage = "https://commons.wikimedia.org/wiki/Special:Redirect/file/%C4%90%C3%ACnh_l%C3%A0ng_Kim_Long.jpg";
    private const string VoiReImage = "https://commons.wikimedia.org/wiki/Special:Redirect/file/Voi_r%C3%A9_1.jpg";
    private const string HaiVanQuanImage = "https://commons.wikimedia.org/wiki/Special:Redirect/file/Di_t%C3%ADch_H%E1%BA%A3i_V%C3%A2n_Quan.JPG";
    private const string DragonBoatSource = "https://commons.wikimedia.org/wiki/File:Dragon_boat_on_the_Perfume_River_(25606150197).jpg";
    private const string BaoVinhSource = "https://commons.wikimedia.org/wiki/File:Bao_Vinh_street.jpg";
    private const string ThanhTanSource = "https://huetourism.gov.vn/suoi-khoang-nong-thanh-tan/?pid=MjI5NDB8Y3NkbGRs0";
    private const string RuChaSource = "https://hue.gov.vn/Du-dia-chi/tb/Rung-ngap-man-Ru-Cha-120592";
    private const string ThuyBieuSource = "https://huetourism.gov.vn/XUAN-DAI--NGOI-NHA-RUONG-CO-THUY-BIEU-XU-HUE.html/?pid=MTc3NzZ8Y3NkbGRs0";
    private const string ThuyXuanSource = "https://huetourism.gov.vn/lang-huong-tram-tuoi-lam-du-lich/?pid=MjI0Mzl8Y3NkbGRs0";
    private const string ThuyTienFlowersSource = "https://commons.wikimedia.org/wiki/File:Hoa_giay_Thanh_Tien.JPG";
    private const string GiaLongSource = "https://commons.wikimedia.org/wiki/File:Hue_Vietnam_Tomb-of-Emperor-Gia-Long-07.jpg";
    private const string VongCanhSource = "https://huetourism.gov.vn/len-doi-vong-canh-ngam-tron-hue-mong-mo-giua-troi-xanh-song-biec/?pid=MjM3OTl8Y3NkbGRs0";
    private const string AnHienSource = "https://huetourism.gov.vn/nha-vuon-an-hien-binh-lang-cua-xu-hue/?pid=MjMwMjh8Y3NkbGRs0";
    private const string VancuSource = "https://huetourism.gov.vn/quang-dien-van-cu-tinh-hoa-nghe-bun-diem-nhan-an-tuong-cua-chuong-trinh-%E2%80%9Chue-kinh-do-am-thuc%E2%80%9D/?pid=MjM2OTF8Y3NkbGRs0";
    private const string HonChaoSource = "https://huetourism.gov.vn/Dao-Hon-Chao-Danh-thang-dac-biet-cua-bien-xu-Hue.html/?pid=MTg3MjN8Y3NkbGRs0";
    private const string KimLongCommunalHouseSource = "https://commons.wikimedia.org/wiki/File:%C4%90%C3%ACnh_l%C3%A0ng_Kim_Long.jpg";
    private const string VoiReSource = "https://commons.wikimedia.org/wiki/File:Voi_r%C3%A9_1.jpg";
    private const string HaiVanQuanSource = "https://commons.wikimedia.org/wiki/File:Di_t%C3%ADch_H%E1%BA%A3i_V%C3%A2n_Quan.JPG";

    private static readonly IReadOnlyDictionary<string, TourGalleryPhoto[]> PhotosByTour =
        new Dictionary<string, TourGalleryPhoto[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["HT-DAINOI01"] =
            [
                Local("/images/imperial-city-2025.jpg", "Hoàng thành Huế"),
                Local("/images/ngo-mon-gate.jpg", "Cổng Ngọ Môn"),
                Local("/images/khai-dinh-tomb.jpg", "Lăng Khải Định"),
                Local("/images/tu-duc-tomb.jpg", "Lăng Tự Đức")
            ],
            ["HT-TAMGIANG02"] =
            [
                Local("/images/tam-giang-lagoon.jpg", "Phá Tam Giang"),
                Local("/images/tam-giang-2024.jpg", "Phá Tam Giang – Cầu Hai")
            ],
            ["HT-SONGHUONG03"] =
            [
                Remote(DragonBoatImage, "Thuyền rồng trên sông Hương", "Mig Gilbert, CC BY-SA 2.0", DragonBoatSource),
                Local("/images/truong-tien-2024.jpg", "Cầu Trường Tiền nhìn từ sông Hương")
            ],
            ["HT-THUYBIEU04"] =
            [
                Remote(ThuyBieuGardenHouseImage, "Nhà rường cổ Xuân Đài, Thủy Biều", "Đoàn Anh Thái – Cổng thông tin Du lịch Huế", ThuyBieuSource),
                Remote(ThuyBieuFruitGardenImage, "Vườn cây trái tại nhà rường Xuân Đài", "Đoàn Anh Thái – Cổng thông tin Du lịch Huế", ThuyBieuSource)
            ],
            ["HT-AMTHUCDEM05"] =
            [
                Local("/images/dong-ba-market-2024.jpg", "Quầy ẩm thực tại chợ Đông Ba"),
                Local("/images/bun-bo-hue.jpg", "Bún bò Huế"),
                Local("/images/bun-bo-hue-2024.jpg", "Bún bò Huế tại chợ Đông Ba")
            ],
            ["HT-BACHMA06"] =
            [
                Local("/images/bach-ma-national-park.jpg", "Bình minh trên dãy Bạch Mã"),
                Local("/images/bach-ma-panorama-cc0.jpg", "Toàn cảnh núi Bạch Mã"),
                Local("/images/bach-ma-waterfall.jpg", "Thác Đỗ Quyên, Vườn quốc gia Bạch Mã")
            ],
            ["HT-TUDUC07"] =
            [
                Remote(ThuyXuanIncenseImage, "Làng hương Thủy Xuân", "Cổng thông tin Du lịch Huế (theo Đại Đoàn Kết)", ThuyXuanSource),
                Remote(VongCanhImage, "Sông Hương và lăng tẩm nhìn từ đồi Vọng Cảnh", "Hải Vân – Cổng thông tin Du lịch Huế", VongCanhSource)
            ],
            ["HT-THANHTOAN08"] =
            [
                Local("/images/thanh-toan-bridge.jpg", "Cầu ngói Thanh Toàn"),
                Local("/images/thanh-toan-2018.jpg", "Lối vào cầu ngói Thanh Toàn"),
                Remote(ThuyTienFlowersImage, "Hoa giấy làng Thanh Tiên", "Lưu Ly, CC BY-SA 3.0", ThuyTienFlowersSource)
            ],
            ["HT-LANGCO09"] =
            [
                Local("/images/lang-co-lagoon.jpg", "Đầm Lập An, Lăng Cô"),
                Remote(HaiVanQuanImage, "Di tích Hải Vân Quan", "LÊ TẤN LỘC, CC BY-SA 3.0", HaiVanQuanSource)
            ],
            ["HT-MINHMANG10"] =
            [
                Local("/images/minh-mang-2024.jpg", "Lăng Minh Mạng"),
                Remote(GiaLongTombImage, "Lăng Gia Long", "CEphoto, Uwe Aranas, CC BY-SA 3.0", GiaLongSource),
                Remote(VongCanhImage, "Sông Hương và lăng tẩm nhìn từ đồi Vọng Cảnh", "Hải Vân – Cổng thông tin Du lịch Huế", VongCanhSource)
            ],
            ["HT-NONLA11"] =
            [
                Local("/images/hue-conical-hat.jpg", "Nghệ nhân làm nón lá ở vùng quê Huế"),
                Remote(ThuyTienFlowersImage, "Hoa giấy làng Thanh Tiên", "Lưu Ly, CC BY-SA 3.0", ThuyTienFlowersSource)
            ],
            ["HT-SUOIVOI12"] =
            [
                Local("/images/lang-co-lagoon.jpg", "Đầm Lập An, Lăng Cô")
            ],
            ["HT-DAINOI-THIENMU13"] =
            [
                Remote(AnHienHouseImage, "Nhà vườn An Hiên", "Minh Toàn – Cổng thông tin Du lịch Huế", AnHienSource),
                Remote(AnHienGardenImage, "Vườn cây và hồ nước nhà vườn An Hiên", "Minh Toàn – Cổng thông tin Du lịch Huế", AnHienSource),
                Local("/images/imperial-city-2025.jpg", "Hoàng thành Huế"),
                Local("/images/ngo-mon-gate.jpg", "Cổng Ngọ Môn"),
                Local("/images/thien-mu-pagoda.jpg", "Chùa Thiên Mụ")
            ],
            ["HT-RUCHATAMGIANG14"] =
            [
                Remote(RuChaImage, "Rừng ngập mặn Rú Chá", "Cổng thông tin điện tử Thành phố Huế", RuChaSource),
                Local("/images/tam-giang-lagoon.jpg", "Cảnh quan phá Tam Giang")
            ],
            ["HT-SONGHUONGAMTHUC15"] =
            [
                Local("/images/truong-tien-2024.jpg", "Cầu Trường Tiền"),
                Local("/images/bun-bo-hue-2024.jpg", "Ẩm thực tại chợ Đông Ba"),
                Local("/images/bun-bo-hue.jpg", "Bún bò Huế")
            ],
            ["HT-LANGMINHMANG16"] =
            [
                Local("/images/minh-mang-2024.jpg", "Lăng Minh Mạng"),
                Local("/images/khai-dinh-tomb.jpg", "Lăng Khải Định"),
                Remote(GiaLongTombImage, "Lăng Gia Long", "CEphoto, Uwe Aranas, CC BY-SA 3.0", GiaLongSource)
            ],
            ["HT-THUANAN-TAMGIANG17"] =
            [
                Local("/images/tam-giang-2024.jpg", "Đầm Chuồn, phá Tam Giang"),
                Local("/images/tam-giang-lagoon.jpg", "Cảnh quan vùng đầm phá"),
                Local("/images/thuan-an-beach-2023.jpg", "Bãi biển Thuận An")
            ],
            ["HT-HOQUYEN18"] =
            [
                Local("/images/ho-quyen-2008.jpg", "Di tích Hổ Quyền"),
                Remote(VoiReImage, "Điện Voi Ré", "Lưu Ly, phạm vi công cộng", VoiReSource),
                Local("/images/tu-duc-tomb.jpg", "Lăng Tự Đức")
            ],
            ["HT-HONCHEN19"] =
            [
                Remote(VongCanhImage, "Sông Hương và lăng tẩm nhìn từ đồi Vọng Cảnh", "Hải Vân – Cổng thông tin Du lịch Huế", VongCanhSource)
            ],
            ["HT-THANHTAN20"] =
            [
                Remote(ThanhTanHotSpringImage, "Dòng suối khoáng nóng Thanh Tân", "Bảo Khánh – Alba Wellness Valley", ThanhTanSource),
                Remote(ThanhTanOnsenImage, "Không gian tắm Onsen và Spa", "Bảo Khánh – Alba Wellness Valley", ThanhTanSource)
            ],
            ["HT-VANCU21"] =
            [
                Remote(BaoVinhImage, "Phố cổ Bao Vinh", "Sgnpkd, CC BY-SA 4.0", BaoVinhSource),
                Remote(VancuBunImage, "Trình diễn nghề làm bún Vân Cù", "Cổng thông tin Du lịch Huế – Nguyễn Thúc Nhân", VancuSource),
                Remote(KimLongCommunalHouseImage, "Đình làng Kim Long", "Christophe95, CC BY-SA 4.0", KimLongCommunalHouseSource)
            ],
            ["HT-HONCHAO22"] =
            [
                Local("/images/lang-co-lagoon.jpg", "Đầm Lập An, Lăng Cô")
            ],
            ["HT-DI_SAN_DEM23"] =
            [
                Local("/images/imperial-city-2025.jpg", "Hoàng thành Huế"),
                Local("/images/ngo-mon-gate.jpg", "Cổng Ngọ Môn")
            ],
            ["HT-DISANDEM23"] =
            [
                Local("/images/imperial-city-2025.jpg", "Hoàng thành Huế"),
                Local("/images/ngo-mon-gate.jpg", "Cổng Ngọ Môn")
            ]
        };

    private static readonly IReadOnlyDictionary<string, string> CoverCorrections =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["HT-SONGHUONG03"] = DragonBoatImage,
            ["HT-THUYBIEU04"] = ThuyBieuGardenHouseImage,
            ["HT-DAINOI-THIENMU13"] = AnHienHouseImage,
            ["HT-VANCU21"] = BaoVinhImage,
            ["HT-RUCHATAMGIANG14"] = RuChaImage,
            ["HT-HONCHAO22"] = HonChaoImage
        };

    private static readonly IReadOnlyDictionary<string, string> CoverCaptions =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["HT-DAINOI01"] = "Hoàng thành Huế",
            ["HT-TAMGIANG02"] = "Đầm Tam Giang – Cầu Hai lúc hoàng hôn",
            ["HT-SONGHUONG03"] = "Thuyền rồng trên sông Hương",
            ["HT-THUYBIEU04"] = "Nhà rường cổ Xuân Đài, Thủy Biều",
            ["HT-AMTHUCDEM05"] = "Ẩm thực tại chợ Đông Ba",
            ["HT-BACHMA06"] = "Thác Đỗ Quyên, Vườn quốc gia Bạch Mã",
            ["HT-TUDUC07"] = "Lăng Tự Đức",
            ["HT-THANHTOAN08"] = "Cầu ngói Thanh Toàn",
            ["HT-LANGCO09"] = "Vịnh Lăng Cô",
            ["HT-MINHMANG10"] = "Lăng Minh Mạng",
            ["HT-NONLA11"] = "Nghệ nhân làm nón lá ở Huế",
            ["HT-SUOIVOI12"] = "Vịnh Lăng Cô",
            ["HT-DAINOI-THIENMU13"] = "Nhà vườn An Hiên",
            ["HT-RUCHATAMGIANG14"] = "Rừng ngập mặn Rú Chá",
            ["HT-SONGHUONGAMTHUC15"] = "Chợ Đông Ba",
            ["HT-LANGMINHMANG16"] = "Lăng Minh Mạng",
            ["HT-THUANAN-TAMGIANG17"] = "Bãi biển Thuận An",
            ["HT-HOQUYEN18"] = "Di tích Hổ Quyền",
            ["HT-HONCHEN19"] = "Điện Hòn Chén",
            ["HT-THANHTAN20"] = "Suối khoáng nóng Thanh Tân",
            ["HT-VANCU21"] = "Phố cổ Bao Vinh",
            ["HT-HONCHAO22"] = "Đảo Hòn Chảo (Sơn Chà)",
            ["HT-DI_SAN_DEM23"] = "Đại Nội Huế về đêm",
            ["HT-DISANDEM23"] = "Đại Nội Huế về đêm"
        };

    public static string GetCoverUrl(Tour tour) =>
        GetCoverPhoto(tour).Url;

    public static string GetCoverCaption(Tour tour) =>
        GetCoverPhoto(tour).Caption;

    public static TourGalleryPhoto GetCoverPhoto(Tour tour) => CreateCover(tour);

    public static IReadOnlyList<TourGalleryPhoto> GetPhotos(Tour tour)
    {
        var cover = CreateCover(tour);
        if (!PhotosByTour.TryGetValue(tour.Code, out var additionalPhotos))
        {
            return [cover];
        }

        return [cover, .. additionalPhotos.Where(photo => !string.Equals(photo.Url, cover.Url, StringComparison.OrdinalIgnoreCase))];
    }

    private static TourGalleryPhoto CreateCover(Tour tour)
    {
        if (string.Equals(tour.Code, "HT-THANHTAN20", StringComparison.OrdinalIgnoreCase))
        {
            return Remote(tour.ImageUrl, "Suối khoáng nóng Thanh Tân", "Bảo Khánh – Alba Wellness Valley", ThanhTanSource);
        }

        if (CoverCorrections.TryGetValue(tour.Code, out var correctedUrl))
        {
            if (correctedUrl == DragonBoatImage)
            {
                return Remote(correctedUrl, "Thuyền rồng trên sông Hương", "Mig Gilbert, CC BY-SA 2.0", DragonBoatSource);
            }

            if (correctedUrl == BaoVinhImage)
            {
                return Remote(correctedUrl, "Phố cổ Bao Vinh", "Sgnpkd, CC BY-SA 4.0", BaoVinhSource);
            }

            if (correctedUrl == ThuyBieuGardenHouseImage)
            {
                return Remote(correctedUrl, "Nhà rường cổ Xuân Đài, Thủy Biều", "Đoàn Anh Thái – Cổng thông tin Du lịch Huế", ThuyBieuSource);
            }

            if (correctedUrl == AnHienHouseImage)
            {
                return Remote(correctedUrl, "Nhà vườn An Hiên", "Minh Toàn – Cổng thông tin Du lịch Huế", AnHienSource);
            }

            if (correctedUrl == HonChaoImage)
            {
                return Remote(correctedUrl, "Đảo Hòn Chảo (Sơn Chà)", "Cổng thông tin Du lịch Huế (theo Công Thương)", HonChaoSource);
            }

            return Remote(correctedUrl, "Rừng ngập mặn Rú Chá", "Cổng thông tin điện tử Thành phố Huế", RuChaSource);
        }

        var caption = CoverCaptions.TryGetValue(tour.Code, out var knownCaption)
            ? knownCaption
            : tour.Title;

        return new TourGalleryPhoto(tour.ImageUrl, caption);
    }

    private static TourGalleryPhoto Local(string url, string caption) => new(url, caption);

    private static TourGalleryPhoto Remote(string url, string caption, string credit, string creditUrl) =>
        new(url, caption, credit, creditUrl);
}

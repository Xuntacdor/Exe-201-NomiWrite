# Mini plan — khôi phục UI NomiWrite

Mục tiêu: giao diện luyện TOEIC/IELTS gần gũi, nhất quán, dễ tập trung viết và đọc phản hồi lâu; hỗ trợ sáng/tối đầy đủ. Đây là kế hoạch triển khai, chưa thay đổi UI.

## Phạm vi và hiện trạng

- Tập trung `NomiWrite.Frontend`: CSS/theme, typography, bố cục, component trình bày, responsive và accessibility. Chỉ thêm state cho theme và lưu lựa chọn client-side; không thêm thư viện UI lớn.
- Giữ nguyên backend, API, schema/types nghiệp vụ, dữ liệu thật, hooks/services/cache, routing, validation, handlers, bindings, auth, chấm/nộp bài và lưu nháp. Không sửa lỗi nghiệp vụ từ báo cáo review trong đợt UI này; bảo toàn mọi thay đổi sẵn có.
- Code hiện dùng Next.js, Tailwind v4, Nunito hỗ trợ tiếng Việt và Lucide; màu hard-code xanh/cam/tím chưa đồng bộ, còn gradient/glow và shadow trang trí. Chưa tìm thấy theme provider trong `app`/`lib`.

## Khảo sát tham chiếu — cập nhật 27/09/2026

Đã thử kết nối Browser nhưng không có browser khả dụng. Khảo sát gồm nội dung công khai và ảnh YouPass người dùng cung cấp ngày 27/09/2026; chưa trực tiếp thao tác editor hoặc lấy CSS thực tế. Một số kết quả web là bản được công cụ tìm kiếm thu thập trước đó, không chứng minh UI hiện tại.

| Nguồn | Thông tin đã đọc được | Áp dụng chọn lọc vào NomiWrite |
| --- | --- | --- |
| [YouPass – Writing Task 1](https://youpass.vn/luyen-thi/ielts/writing/task-1?status=unfinished&topic=LINE) | Nội dung mô tả luyện theo đề/chủ đề, hướng dẫn từng bước bằng tiếng Việt và chấm chữa chi tiết. Phần danh sách/editor động không xuất hiện trong kết quả đọc. | Giữ YouPass là tham chiếu chính về cách hỗ trợ người học: nhãn dễ hiểu, đề rõ, hướng dẫn/feedback gần thao tác cần làm; chỉ trình bày dữ liệu và chức năng đang có. |
| [YouPass – Task 2 Builder](https://builder.youpass.vn/task2/) | Kết quả tìm kiếm có danh mục chủ đề và dạng bài; mở trực tiếp trả 502. Trang chủ YouPass trả 403. | Chưa chốt bố cục từ Builder; không thêm lịch học, gamification hoặc bảng tiến độ theo đoạn trích tìm kiếm. |
| [STUDY4 – Thư viện đề](https://study4.com/tests/) | Có ô tìm kiếm, tên đề, thời lượng, số phần/câu hỏi, nhãn loại bài, liên kết chi tiết và phân trang. Mở chi tiết một đề không thành công. | Làm rõ tên đề → thông tin phụ → hành động trong danh sách hiện có; đưa bộ lọc hiện có gần tiêu đề. Không bổ sung tìm kiếm/phân trang hay số liệu nếu NomiWrite chưa có. |
| [Write & Improve – Free](https://writeandimprove.com/free) | Nội dung hướng dẫn theo chuỗi chọn đề → viết → nhận feedback → sửa bài; mô tả phản hồi ở mức từ/câu. Liên kết Start writing dẫn tới [workbooks](https://writeandimprove.com/workbooks), nhưng trang này không trả nội dung đọc được. | Ưu tiên editor và khả năng đối chiếu bài với feedback; giữ hành động chính rõ ràng, không để trang trí cạnh tranh với nội dung. Chưa xác nhận cách chia cột hay highlight của editor tham chiếu. |

### Kiểm tra riêng light/dark — 27/09/2026

| Website | Bằng chứng về light mode | Bằng chứng về dark mode |
| --- | --- | --- |
| YouPass | Ảnh người dùng cung cấp của trang khóa học Intensive: nền kem rất nhạt, chữ tiêu đề gần đen, CTA cam; thanh điều hướng xanh mint và nhãn xanh. Đây là trang khóa học, chưa phải editor. Không lấy màu xanh của thanh trình duyệt làm màu website. | Theo xác nhận của người dùng, YouPass chỉ có tông sáng. Chỉ dùng YouPass làm tham chiếu light; không tiếp tục tìm dark mode của YouPass. |
| STUDY4 | Đọc được thư viện đề và cấu trúc nội dung; kết quả dạng text không đủ để xác định màu CSS. | Tìm kiếm chưa cung cấp tài liệu chính thức liên quan đến dark mode của chính STUDY4. Không kết luận là website không có dark mode. |
| Write & Improve | [Tài liệu chính thức về highlight](https://help.writeandimprove.com/en/articles/806462-sentence-feedback-what-is-the-coloured-highlighting-on-sentences-in-write-improve) mô tả câu tốt có nền trắng, câu cần cải thiện có nền màu. Đây là mô tả feedback trong tài liệu từ 2019, không phải kiểm chứng theme hiện tại. | Chưa xác nhận được theme dark của editor; trang workbooks không trả nội dung động và Browser chưa kết nối. |

Kết luận khảo sát: ảnh YouPass xác nhận hướng light nền dịu/chữ đen/CTA rõ; NomiWrite chọn màu ấm theo yêu cầu, không sao chép xanh mint, mascot hay hero lớn sang màn hình học. Dark mode được thiết kế riêng dựa trên hướng dẫn bên dưới. Các mã màu NomiWrite là đề xuất, không phải màu lấy chính xác từ ảnh YouPass.

### Cơ sở thiết kế dark mode

- [Google Material – Design a dark theme](https://codelabs.developers.google.com/codelabs/design-material-darktheme): dùng nền tối, bề mặt nổi sáng hơn để phân cấp; accent sáng hơn và ít bão hòa hơn; điều chỉnh màu chữ theo bề mặt thay vì đảo màu. Áp dụng cho NomiWrite bằng nền than thiên ấm, editor sáng hơn nền một mức, menu/modal sáng hơn editor; cam đào/hồng phấn/vàng kem chỉ nhấn ở khu vực cần thiết. Không bắt buộc dùng màu xám mẫu của Material hoặc thêm thư viện Material.
- Chữ nội dung dark dùng kem dịu hơi giảm độ sáng so với nền kem light để tránh cả màn hình chữ quá rực; hồng phấn/cam đào/vàng nhạt dành cho tiêu đề, nhãn và trạng thái chọn. Nút nền pastel vẫn dùng chữ đen ở cả hai theme. Đây là lựa chọn riêng cho NomiWrite theo hướng dẫn, không phải bảng màu quy định sẵn của Google.
- [WCAG – Contrast Minimum](https://www.w3.org/WAI/WCAG21/Understanding/contrast-minimum): chữ thường ít nhất 4.5:1, chữ lớn ít nhất 3:1. [WCAG – Non-text Contrast](https://www.w3.org/WAI/WCAG22/Understanding/non-text-contrast.html): dấu hiệu thị giác cần để nhận biết control/trạng thái cần đạt 3:1 với màu liền kề. Viền trang trí không mặc nhiên phải đạt 3:1; viền input/focus phải có token riêng đủ rõ khi cần để nhận biết control.
- Kiểm tra màu feedback và selection riêng ở cả hai theme; bổ sung nhãn/icon sẵn có để màu không là dấu hiệu duy nhất. Không dùng filter invert, glow hoặc gradient để tạo dark mode.

### Hướng thiết kế đề xuất sau khảo sát

Các thông số sau là đề xuất cho NomiWrite theo yêu cầu và code hiện tại, không phải màu/kích thước trích từ website tham chiếu:

- **Màu — theo yêu cầu cập nhật:** dùng bảng màu ấm pastel; không dùng xanh dương hoặc các tông lạnh làm chủ đạo. Light lấy kem ấm làm nền chung, hồng phấn và cam đào nhạt cho bề mặt/điểm nhấn, vàng kem cho vùng hỗ trợ; chữ đen trung tính dễ đọc. Dark dùng nền tối thiên nâu, chữ chính màu kem và chữ nhấn hồng phấn/cam đào/vàng nhạt tương ứng với các màu nền light. Giữ vai trò màu nhất quán, không rải cả ba màu lên mọi thành phần; màu trạng thái chỉ dùng khi có ý nghĩa.
- **Chữ và khoảng cách:** giữ Nunito hỗ trợ tiếng Việt, nội dung 16px/line-height khoảng 1.65, nhãn phụ 13–14px; nhịp khoảng cách 4/8/12/16/24/32px, bo góc 8–12px, ưu tiên viền hơn shadow. Không thu nhỏ nội dung học để nhét thêm thành phần.
- **Sidebar:** nhóm các link hiện có thành học tập, theo dõi và cá nhân bằng nhãn trình bày; mục active dùng nền nhạt cùng accent. Nút theme gần cuối, giữ logout và đích link. Kiểm tra chiều cao/scroll khi thu gọn và trên mobile.
- **Viết:** desktop đề khoảng 40%, editor 60% nếu vừa với cấu trúc hiện có; màn hình nhỏ xếp đề trước editor. Toolbar gọn cho đồng hồ/số từ và hành động đang có, không remount editor hoặc thay bindings/handlers. Không thêm banner lớn hay cơ chế tương tác mới để thu gọn đề.
- **Kết quả:** điểm tổng và tiêu chí ở đầu; bài viết và nhận xét có ranh giới rõ, highlight đọc được cả sáng/tối. Giữ thứ tự và liên kết lỗi hiện có; giảm card lồng và mảng màu lớn.
- **Danh sách/tổng quan:** ưu tiên tên, trạng thái, thông tin có thật và hành động chính; empty/loading/error cùng hệ thống style. Study Guide/Quiz/Vocabulary dùng chung tokens, không sửa nghiệp vụ trong đợt này.

### Bảng màu khởi điểm

Các mã màu dưới đây dùng để triển khai thử và kiểm tra tương phản, chưa phải kết quả kiểm chứng trên UI:

| Vai trò | Light mode | Dark mode |
| --- | --- | --- |
| Nền trang | Kem ấm `#FFF8F2` | Nâu than `#211B19` |
| Bề mặt editor/nội dung | Trắng kem `#FFFCF8` | Nâu tối `#2C2421` |
| Bề mặt nổi (menu/modal) | Trắng kem `#FFFCF8` | Nâu xám tối `#382E29` |
| Chữ chính | Đen dịu `#1A1A1A` | Kem dịu `#F2E5D8` |
| Chữ phụ | Xám đậm trung tính `#595959` | Be ấm `#CDB9AE` |
| Điểm nhấn chính/active | Nền cam đào `#FBE2D2`, chữ đen `#1A1A1A` | Nền nâu cam `#493127`, chữ cam đào `#FBE2D2` |
| Điểm nhấn phụ | Nền hồng phấn `#F9E5E8`, chữ đen `#1A1A1A` | Nền hồng nâu tối `#422C32`, chữ hồng phấn `#F9E5E8` |
| Vùng hỗ trợ | Nền vàng kem `#FFF1CB`, chữ đen `#1A1A1A` | Nền nâu vàng tối `#403522`, chữ vàng kem `#FFF1CB` |
| Viền phân cách | Be nhạt `#E8D8CC` | Nâu xám `#59483F` |

- Cam đào là accent chính; hồng phấn/vàng kem dùng có tiết chế. Vùng viết và đoạn phản hồi dài giữ bề mặt đồng nhất để đọc lâu; dark dùng kem làm chữ nội dung, các màu pastel còn lại cho tiêu đề/nhãn/điểm nhấn theo vai trò.
- Không dùng chữ pastel trên nền sáng hoặc chữ trắng trên nút pastel. Nút nền pastel dùng chữ đen; focus ring dùng sắc ấm đủ đậm ở light và đủ sáng ở dark.
- Kiểm tra từng cặp chữ–nền, hover/active/focus và highlight lỗi ở cả hai theme: chữ thường tối thiểu 4.5:1; chữ lớn và thành phần tương tác cần tương phản phù hợp. Không chỉ đảo màu tự động giữa hai chế độ.
- Đã tính tương phản sRGB cho một số cặp màu đặc: chữ light `#1A1A1A` trên `#FFF8F2` đạt **16.54:1**; chữ dark `#F2E5D8` trên editor `#2C2421` đạt **12.29:1**, trên modal `#382E29` đạt **10.67:1**; chữ phụ `#CDB9AE` trên modal đạt **7.01:1**. Đây là kiểm tra mã màu, chưa phải kiểm tra UI thực tế; opacity, hover, ảnh nền và các control còn phải kiểm tra khi triển khai.

## Thứ tự thực hiện

1. [ ] **Hoàn tất khảo sát trực quan và chụp hiện trạng.** Đã khảo sát nội dung công khai ở bảng trên; chưa hoàn tất phần trực quan. Khi Browser khả dụng, mở UI thật ở desktop/mobile, ưu tiên `/write`, `/result`, `/dashboard`, `/history`, rồi thử lại màn hình học của YouPass/Write & Improve. Đối chiếu hướng đề xuất, chốt tokens và ghi rõ trang nào xem được; không chặn chỉnh styles nếu tham chiếu vẫn không truy cập được. Kiểm tra hướng dẫn dự án/working tree trước khi triển khai.
2. [ ] **Chuẩn hóa styles và sáng/tối.** Sửa `app/globals.css`, `app/layout.tsx` và shared UI: tokens cho nền, bề mặt, chữ, viền, accent, focus, trạng thái và highlight lỗi; bo góc vừa phải, ít bóng, bỏ gradient/glow/animation trang trí. Dùng lại provider nếu có, nếu chưa có thì thêm cơ chế nhỏ: lần đầu theo hệ thống, lưu lựa chọn localStorage, xử lý SSR/hydration và hạn chế nháy theme; không remount editor/form khi đổi theme.
3. [ ] **Sidebar và khung trang.** Chỉnh `app/components/AppSidebar.tsx`, `AppShell.tsx`: nhóm mục hiện có, active rõ, khoảng cách gọn, mobile không che nội dung. Đặt nút Sáng/Tối gần cuối sidebar, icon mặt trời/mặt trăng, nhãn tiếng Việt ngắn; có accessible label, tooltip khi thu gọn, focus visible và thao tác bàn phím. Giữ nguyên đích điều hướng và hành vi.
4. [ ] **Ưu tiên màn hình học.** `/write`: editor là trọng tâm, đề và bài có ranh giới rõ, hai cột desktop nếu phù hợp, xếp dọc mobile; đồng hồ/số từ/lưu/nộp dễ tìm, không thêm hero. `/result`: phân cấp điểm, tiêu chí, nhận xét và lỗi; giữ nguyên thứ tự có ý nghĩa, liên kết lỗi và nội dung thật. Sau đó đồng bộ dashboard/history, guide/study-guide, quiz/vocabulary và các trang còn lại bằng cùng tokens; không thêm card lồng nhau, số liệu hay tính năng giả.
5. [ ] **Kiểm chứng và bàn giao.** Chạy lint/build Frontend; rà diff chỉ liên quan trình bày/theme. Kiểm tra desktop 1440px, mobile 390px và 320px ở cả hai theme: sidebar/header/editor/form/menu/modal/bảng/tooltip/feedback, hover/focus/active/disabled, không tràn ngang hay che nút. Thử nhập → đổi theme (giữ nội dung, con trỏ/focus editor) → chuyển trang/tải lại (giữ theme và draft theo cơ chế cũ) → nộp bài → xem kết quả bằng dữ liệu test phù hợp. Chụp/xem lại màn hình nếu có Browser; báo đúng phần đã/chưa kiểm tra, file sửa và kết quả.

Hoàn thành khi màn hình học dễ đọc và nhất quán ở cả hai theme, thao tác hiện có được bảo toàn, lint/build đạt, và mọi giới hạn kiểm thử được ghi rõ. Không tuyên bố luồng nghiệp vụ đã đạt chỉ dựa vào build.

## K?t qu? tri?n khai ? 27/09/2026

- Nh?nh UI t?ch t? `0b0eafe`: `PhmHai0702/fe/warm-ui-recovery`.
- D?ng CSS tokens xuy?n su?t frontend: n?n kem/ch? ?en ? light; n?n t?i ?m/ch? kem ? dark; accent cam ??o, h?ng ph?n v? v?ng kem. M?u l?i/th?nh c?ng v?n ph?n bi?t theo ? ngh?a; b? gradient/glow v? c?c kh?i blur trang tr?.
- Sidebar c? nh?m ?i?u h??ng, active r?, thanh icon g?n tr?n mobile v? n?t S?ng/T?i c? nh?n truy c?p. Gi? routes v? logout; theme kh?i t?o tr??c paint, theo h? th?ng khi ch?a ch?n, l?u localStorage v? c?p nh?t gi?a c?c tab.
- Editor/?? chia 40?60 tr?n desktop, x?p d?c d??i 1024px; textarea c? accessible label v? v?ng nh?p ?? cao. K?t qu? d?ng n?n nh?, ch? ph?n h?i l?n h?n; c?c trang h?c, t?i kho?n, thanh to?n v? landing d?ng chung m?u theo theme.
- ??i theme ch? c?p nh?t thu?c t?nh tr?n html, kh?ng thay key ho?c remount c?y n?i dung; pointer toggle gi? focus editor. Test m? ph?ng ki?m tra c?c nh?nh theme, storage b? ch?n, ??ng b? tab v? editor; kh?ng coi ?? l? ki?m ch?ng thao t?c th?t trong browser.
- ?? ??t `npm run lint`, `npm run build`, `npm run test:theme`, `git diff --check`. ??i chi?u AST x?c nh?n API calls/hooks/form bindings/handlers/hrefs c?a 17 trang kh?ng ??i.
- C?n ch?: ?nh desktop/mobile (1440/390/320px), ki?m tra tr?n/che n?t, focus/selection v? nh?p ? ??i theme ? t?i l?i ? n?p ? xem k?t qu? v?i t?i kho?n test. Browser hi?n kh?ng c? k?t n?i n?n ch?a ??nh d?u ho?n th?nh b??c 1 v? 5.
- Backend/Admin v? c?c l?i nghi?p v? trong b?o c?o review kh?ng thu?c diff UI. Vi?c x?a t?i li?u plan c? ???c l?u ri?ng ?? revert UI kh?ng kh?i ph?c nh?m t?i li?u ?? b?.

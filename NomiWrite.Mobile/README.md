# NomiWrite Mobile

Ứng dụng Flutter Android/iOS dành cho người học của hệ thống NomiWrite. App dùng
chung API Gateway và mô hình dữ liệu với `NomiWrite.Frontend`.

## Tính năng

- Đăng ký, đăng nhập, lưu phiên an toàn và tự refresh token.
- Dashboard tiến độ, streak, band score và bài viết gần đây.
- Chọn loại bài/đề, soạn bài, chế độ timed, nộp và xem phản hồi AI.
- Lịch sử bài viết và điểm theo từng tiêu chí.
- Từ vựng cá nhân, đánh dấu mastered, tạo/làm quiz.
- Study guide cá nhân hóa, hồ sơ, mục tiêu và trình độ.
- Gói Premium và thanh toán VNPay, MoMo, VietQR qua trình duyệt hệ thống.

## Chạy local

Khởi động backend và API Gateway ở cổng `5097`, sau đó:

```powershell
flutter pub get
flutter run
```

Mặc định Android emulator dùng `http://10.0.2.2:5097`; iOS simulator dùng
`http://localhost:5097`. Với thiết bị thật hoặc môi trường khác, truyền URL:

```powershell
flutter run --dart-define=API_BASE_URL=http://192.168.1.10:5097
```

Production nên dùng API HTTPS:

```powershell
flutter build apk --release --dart-define=API_BASE_URL=https://api.example.com
```

## Kiểm tra

```powershell
flutter analyze
flutter test
```

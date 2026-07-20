# Checklist regression API tuần 6

## Thông tin lần chạy

- Ngày chạy: 20/07/2026.
- Môi trường: Development local.
- Base URL: `http://localhost:5291`.
- Công cụ: Newman 6.2.2.
- Collection: `ProductApi-Week6.postman_collection.json`.
- Lệnh chạy:

```bash
npx --yes newman run postman/ProductApi-Week6.postman_collection.json --reporters cli,json --reporter-json-export postman/newman-results.json
```

## Kết quả tổng hợp

| Chỉ số | Đã chạy | Thất bại | Kết quả |
|---|---:|---:|---|
| Iteration | 1 | 0 | PASS |
| Request | 37 | 0 | PASS |
| Test script | 37 | 0 | PASS |
| Pre-request script | 37 | 0 | PASS |
| Assertion | 51 | 0 | PASS |

- Tổng thời gian chạy: 2,1 giây.
- Thời gian phản hồi trung bình: 6 ms.
- Thời gian phản hồi nhỏ nhất/lớn nhất: 2 ms / 32 ms.
- Kết luận: **PASS - toàn bộ 37 request và 51 assertion đều thành công.**

## Checklist chi tiết

### 1. Shared và health

| # | Trường hợp kiểm thử | HTTP mong đợi | Kết quả |
|---:|---|---:|---|
| 1 | Health trả đúng trạng thái và cấu trúc response | 200 | PASS |
| 2 | Info trả đúng thông tin dự án và cấu trúc response | 200 | PASS |

### 2. Category API

| # | Trường hợp kiểm thử | HTTP mong đợi | Kết quả |
|---:|---|---:|---|
| 3 | Tạo Category chính | 201 | PASS |
| 4 | Tạo Category phụ | 201 | PASS |
| 5 | Lấy danh sách Category | 200 | PASS |
| 6 | Lấy chi tiết Category | 200 | PASS |
| 7 | Từ chối tên Category trùng | 409 | PASS |
| 8 | Từ chối tên Category không đủ độ dài sau khi chuẩn hóa khoảng trắng | 400 | PASS |

### 3. Product CRUD và API list

| # | Trường hợp kiểm thử | HTTP mong đợi | Kết quả |
|---:|---|---:|---|
| 9 | Tạo Product và chuẩn hóa tên | 201 | PASS |
| 10 | Lấy chi tiết Product, bao gồm tên Category | 200 | PASS |
| 11 | Search, filter theo Category, sort và pagination | 200 | PASS |
| 12 | Từ chối page/pageSize ngoài giới hạn | 400 | PASS |
| 13 | Từ chối sort field/sort order không hợp lệ | 400 | PASS |
| 14 | Từ chối tên Product không đủ độ dài sau khi chuẩn hóa khoảng trắng | 400 | PASS |
| 15 | Từ chối tạo Product với Category không tồn tại | 400 | PASS |
| 16 | Từ chối Product trùng tên trong cùng Category | 409 | PASS |
| 17 | Cập nhật Product | 200 | PASS |
| 18 | Từ chối xóa Category đang có Product hoạt động | 409 | PASS |

### 4. Soft delete và restore

| # | Trường hợp kiểm thử | HTTP mong đợi | Kết quả |
|---:|---|---:|---|
| 19 | Soft delete Product | 204 | PASS |
| 20 | Product đã soft delete không xuất hiện khi lấy chi tiết | 404 | PASS |
| 21 | Restore Product đã soft delete | 200 | PASS |
| 22 | Product xuất hiện lại sau restore | 200 | PASS |
| 23 | Từ chối restore Product đang hoạt động | 404 | PASS |

### 5. Xung đột khi restore

| # | Trường hợp kiểm thử | HTTP mong đợi | Kết quả |
|---:|---|---:|---|
| 24 | Tạo Category cho kịch bản restore conflict | 201 | PASS |
| 25 | Tạo Product gốc cho kịch bản restore conflict | 201 | PASS |
| 26 | Soft delete Product gốc | 204 | PASS |
| 27 | Tạo Product hoạt động trùng tên | 201 | PASS |
| 28 | Từ chối restore khi tồn tại Product hoạt động trùng tên | 409 | PASS |
| 29 | Soft delete Product trùng đang hoạt động | 204 | PASS |
| 30 | Restore Product gốc sau khi Product trùng bị xóa | 200 | PASS |
| 31 | Soft delete lại Product gốc đã restore | 204 | PASS |
| 32 | Soft delete Category của Product gốc | 204 | PASS |
| 33 | Từ chối restore Product khi Category đã bị xóa | 409 | PASS |

### 6. Cleanup

| # | Trường hợp kiểm thử | HTTP mong đợi | Kết quả |
|---:|---|---:|---|
| 34 | Soft delete Product chính trước khi dọn Category | 204 | PASS |
| 35 | Soft delete Category chính | 204 | PASS |
| 36 | Category đã soft delete không xuất hiện khi lấy chi tiết | 404 | PASS |
| 37 | Soft delete Category phụ | 204 | PASS |

## Ghi chú

- Collection tự tạo hậu tố dữ liệu bằng timestamp nên có thể chạy lại mà không phụ thuộc ID cố định.
- ID được lấy từ response và lưu trong collection variables để dùng cho các request tiếp theo.
- Cleanup sử dụng soft delete vì API hiện chưa có hard-delete endpoint; các bản ghi kiểm thử vẫn tồn tại trong PostgreSQL với `IsDeleted = true`.
- Hai endpoint Health và Info có response riêng, không sử dụng `ApiResponse<T>`. Assertion kiểm tra đúng cấu trúc thực tế của từng endpoint.
- Cảnh báo deprecation `fs.F_OK` đến từ dependency của Newman, không phải từ mã nguồn Product API và không ảnh hưởng kết quả regression.

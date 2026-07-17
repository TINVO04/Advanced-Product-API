# Advanced Product API

Đây là project Product API của tuần 6, được làm tiếp từ project tuần trước.

Trong Day 1 mình tập trung vào việc chuẩn hóa response trả về và xử lý exception chung cho toàn bộ API.

## Công nghệ sử dụng

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Swagger
- GitHub Actions

## Những phần đã làm trong Day 1

- Tạo `ApiResponse<T>` để dùng chung cho response của API.
- Tạo custom exception cho các lỗi `400`, `404` và `409`.
- Tạo `GlobalExceptionMiddleware` để bắt exception tập trung.
- Chuẩn hóa response cho các API Product.
- Chuẩn hóa lỗi validation mặc định của ASP.NET Core.
- Không trả stack trace hoặc nội dung exception nội bộ cho client.
- Test các status code `200`, `201`, `204`, `400`, `404`, `409` và `500`.

## Cấu trúc thư mục chính

```text
src/ProductApi/
├── Common/
│   ├── Exceptions/
│   └── Responses/
├── Controllers/
├── Data/
├── Dtos/
├── Middleware/
├── Models/
├── Repositories/
└── Services/
```

## Cấu trúc response

Response thành công:

```json
{
  "success": true,
  "message": "Product retrieved successfully.",
  "data": {},
  "errors": null,
  "timestampUtc": "2026-07-17T00:00:00Z"
}
```

Response lỗi validation:

```json
{
  "success": false,
  "message": "Validation failed.",
  "data": null,
  "errors": {
    "Name": [
      "Product name is required."
    ]
  },
  "timestampUtc": "2026-07-17T00:00:00Z"
}
```

Riêng API DELETE khi thành công sẽ trả về `204 No Content` và không có body.

## Các endpoint

| Method     | Endpoint               | Chức năng                                                |
| ---------- | ---------------------- | ---------------------------------------------------------- |
| `GET`    | `/api/products`      | Lấy danh sách sản phẩm, có tìm kiếm và phân trang |
| `GET`    | `/api/products/{id}` | Lấy sản phẩm theo ID                                    |
| `POST`   | `/api/products`      | Thêm sản phẩm                                           |
| `PUT`    | `/api/products/{id}` | Cập nhật sản phẩm                                      |
| `DELETE` | `/api/products/{id}` | Xóa sản phẩm                                            |
| `GET`    | `/api/health`        | Kiểm tra API có đang chạy không                       |
| `GET`    | `/api/info`          | Xem thông tin project                                     |

## Các status code đang xử lý

| Status                        | Trường hợp                                   |
| ----------------------------- | ----------------------------------------------- |
| `200 OK`                    | Lấy hoặc cập nhật dữ liệu thành công    |
| `201 Created`               | Tạo sản phẩm thành công                    |
| `204 No Content`            | Xóa sản phẩm thành công                    |
| `400 Bad Request`           | Dữ liệu gửi lên không hợp lệ             |
| `404 Not Found`             | Không tìm thấy sản phẩm                    |
| `409 Conflict`              | Tên sản phẩm bị trùng trong cùng category |
| `500 Internal Server Error` | Có lỗi ngoài dự kiến ở server             |

## Cách chạy project

### Yêu cầu

- .NET 8 SDK
- PostgreSQL

Kiểm tra .NET:

```powershell
dotnet --version
```

### Cấu hình connection string

Project dùng .NET User Secrets để lưu connection string, không lưu mật khẩu PostgreSQL trong source code.

Chạy lệnh sau ở thư mục gốc của project:

```powershell
$password = Read-Host "Mật khẩu PostgreSQL"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=product_api;Username=postgres;Password=$password" --project src/ProductApi/ProductApi.csproj
```

Có thể kiểm tra lại User Secrets bằng lệnh:

```powershell
dotnet user-secrets list --project src/ProductApi/ProductApi.csproj
```

### Chạy migration

Nếu chưa cài `dotnet-ef`:

```powershell
dotnet tool install --global dotnet-ef --version 8.*
```

Cập nhật database:

```powershell
dotnet ef database update --project src/ProductApi/ProductApi.csproj --startup-project src/ProductApi/ProductApi.csproj
```

### Chạy API

```powershell
dotnet run --project src/ProductApi/ProductApi.csproj --launch-profile http
```

Sau khi chạy:

- API: `http://localhost:5291`
- Swagger: `http://localhost:5291/swagger`

## Build project

```powershell
dotnet restore ProductApi.sln
dotnet build ProductApi.sln --configuration Release --no-restore
```

Project có GitHub Actions để tự động restore và build khi push lên `main`, các branch `feature/**` hoặc tạo Pull Request vào `main`.

## Kết quả test Day 1

| Trường hợp test              | Kết quả                          |
| ------------------------------- | ---------------------------------- |
| GET danh sách sản phẩm       | `200 OK`                         |
| GET sản phẩm theo ID          | `200 OK` hoặc `404 Not Found` |
| POST sản phẩm hợp lệ        | `201 Created`                    |
| POST dữ liệu không hợp lệ  | `400 Bad Request`                |
| POST sản phẩm bị trùng tên | `409 Conflict`                   |
| PUT sản phẩm hợp lệ         | `200 OK`                         |
| PUT ID không tồn tại         | `404 Not Found`                  |
| PUT bị trùng tên             | `409 Conflict`                   |
| DELETE sản phẩm tồn tại     | `204 No Content`                 |
| DELETE ID không tồn tại      | `404 Not Found`                  |
| Exception ngoài dự kiến      | `500 Internal Server Error`      |

## Ghi chú Day 1

Một số lỗi mình gặp trong lúc làm:

- Các custom exception ban đầu được đặt sai folder nên mình chuyển về `Common/Exceptions`.
- Validation mặc định của ASP.NET Core trả về `ValidationProblemDetails`, khác với response chung của project nên mình cấu hình lại trong `Program.cs`.
- Mình tạo một endpoint tạm để test lỗi `500` và đã xóa endpoint đó sau khi test xong.

Sau Day 1 mình hiểu rõ hơn cách middleware bắt exception, cách phân chia xử lý giữa controller và service, và lý do không nên trả stack trace cho client.

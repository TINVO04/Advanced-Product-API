# Advanced Product API

Project Web API quản lý sản phẩm và danh mục, được phát triển trong tuần 6 từ project Product API của tuần trước.

Trong tuần này mình tập trung vào các phần thường gặp ở một API thực tế: response dùng chung, xử lý exception tập trung, tìm kiếm và phân trang, quan hệ Product - Category, soft delete, audit fields, logging và regression test bằng Postman.

## Công nghệ sử dụng

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core 8
- PostgreSQL
- Swagger / OpenAPI
- Postman và Newman
- GitHub Actions

## Chức năng chính

### Product

- Lấy danh sách sản phẩm.
- Tìm kiếm theo tên.
- Lọc theo Category.
- Sắp xếp theo ID, tên, giá hoặc số lượng.
- Phân trang và giới hạn `pageSize`.
- Lấy chi tiết sản phẩm kèm tên Category.
- Thêm và cập nhật sản phẩm.
- Kiểm tra Category trước khi lưu.
- Không cho phép trùng tên trong cùng Category.
- Soft delete và khôi phục sản phẩm đã xóa.

### Category

- Lấy danh sách và chi tiết Category.
- Thêm, cập nhật và soft delete Category.
- Không cho phép trùng tên Category.
- Không cho xóa Category nếu vẫn còn Product đang hoạt động.

### Phần dùng chung

- Response thống nhất bằng `ApiResponse<T>`.
- Response phân trang bằng `PagedResult<T>`.
- Xử lý lỗi tập trung bằng `GlobalExceptionMiddleware`.
- Chuẩn hóa lỗi validation mặc định của ASP.NET Core.
- Tự động cập nhật audit fields khi lưu dữ liệu.
- Ghi log method, path, status code và thời gian xử lý request.
- Chuẩn hóa khoảng trắng trong tên Product và Category.
- Kiểm tra độ dài tên sau khi đã chuẩn hóa khoảng trắng.

## Cấu trúc project

```text
.
├── postman/
│   ├── ProductApi-Week6.postman_collection.json
│   └── REGRESSION_CHECKLIST.md
├── src/ProductApi/
│   ├── Common/
│   │   ├── Exceptions/
│   │   ├── Responses/
│   │   ├── Utilities/
│   │   └── Validation/
│   ├── Controllers/
│   ├── Data/
│   ├── Dtos/
│   ├── Middleware/
│   ├── Models/
│   ├── Repositories/
│   └── Services/
├── ProductApi.sln
└── README.md
```

## Các endpoint

### Product API

| Method     | Endpoint                       | Chức năng                                                  |
| ---------- | ------------------------------ | ------------------------------------------------------------ |
| `GET`    | `/api/products`              | Lấy danh sách, tìm kiếm, lọc, sắp xếp và phân trang |
| `GET`    | `/api/products/{id}`         | Lấy Product theo ID                                         |
| `POST`   | `/api/products`              | Thêm Product                                                |
| `PUT`    | `/api/products/{id}`         | Cập nhật Product                                           |
| `PATCH`  | `/api/products/{id}/restore` | Khôi phục Product đã soft delete                         |
| `DELETE` | `/api/products/{id}`         | Soft delete Product                                          |

### Category API

| Method     | Endpoint                 | Chức năng                                                |
| ---------- | ------------------------ | ---------------------------------------------------------- |
| `GET`    | `/api/categories`      | Lấy danh sách Category                                   |
| `GET`    | `/api/categories/{id}` | Lấy Category theo ID                                      |
| `POST`   | `/api/categories`      | Thêm Category                                             |
| `PUT`    | `/api/categories/{id}` | Cập nhật Category                                        |
| `DELETE` | `/api/categories/{id}` | Soft delete Category nếu không còn Product hoạt động |

### Utility API

| Method  | Endpoint        | Chức năng                              |
| ------- | --------------- | ---------------------------------------- |
| `GET` | `/api/health` | Kiểm tra API có đang chạy không     |
| `GET` | `/api/info`   | Xem thông tin project và môi trường |

## Search, filter và pagination

API danh sách Product hỗ trợ các query parameter sau:

| Tham số       | Mặc định | Giá trị hợp lệ                        |
| -------------- | ----------- | ----------------------------------------- |
| `search`     | Không có  | Từ khóa trong tên Product              |
| `categoryId` | Không có  | Số nguyên lớn hơn 0                   |
| `page`       | `1`       | Số nguyên lớn hơn hoặc bằng 1       |
| `pageSize`   | `10`      | Từ 1 đến 100                           |
| `sortBy`     | `id`      | `id`, `name`, `price`, `quantity` |
| `sortOrder`  | `asc`     | `asc`, `desc`                         |

Ví dụ:

```http
GET /api/products?search=phone&categoryId=1&sortBy=price&sortOrder=desc&page=1&pageSize=10
```

Các điều kiện search và filter được dùng chung cho cả truy vấn lấy dữ liệu và truy vấn đếm tổng số bản ghi. Dữ liệu được sắp xếp trước khi gọi `Skip` và `Take` để kết quả giữa các trang ổn định.

## Cấu trúc response

Các API nghiệp vụ sử dụng cùng một dạng response.

Response thành công:

```json
{
  "success": true,
  "message": "Product retrieved successfully.",
  "data": {},
  "errors": null,
  "timestampUtc": "2026-07-20T00:00:00Z"
}
```

Response validation:

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
  "timestampUtc": "2026-07-20T00:00:00Z"
}
```

Response danh sách Product:

```json
{
  "success": true,
  "message": "Products retrieved successfully.",
  "data": {
    "items": [],
    "pageNumber": 1,
    "pageSize": 10,
    "totalItems": 0,
    "totalPages": 0
  },
  "errors": null,
  "timestampUtc": "2026-07-20T00:00:00Z"
}
```

Riêng request `DELETE` thành công trả về `204 No Content` và không có body. Health và Info là hai endpoint tiện ích nên đang dùng response riêng thay vì `ApiResponse<T>`.

## Status code

| Status                        | Trường hợp                                                        |
| ----------------------------- | -------------------------------------------------------------------- |
| `200 OK`                    | Lấy, cập nhật hoặc khôi phục dữ liệu thành công            |
| `201 Created`               | Tạo Product hoặc Category thành công                             |
| `204 No Content`            | Soft delete thành công                                             |
| `400 Bad Request`           | Dữ liệu không hợp lệ hoặc Category không tồn tại            |
| `404 Not Found`             | Không tìm thấy dữ liệu đang hoạt động                       |
| `409 Conflict`              | Trùng dữ liệu, Category còn Product hoặc restore bị xung đột |
| `500 Internal Server Error` | Lỗi ngoài dự kiến ở server                                      |

## Cách chạy project

### Yêu cầu

- .NET 8 SDK
- PostgreSQL

Kiểm tra phiên bản .NET:

```powershell
dotnet --version
```

### Cấu hình database

Project dùng .NET User Secrets để lưu connection string, không đưa mật khẩu PostgreSQL vào source code.

Chạy tại thư mục gốc của project:

```powershell
$password = Read-Host "Mật khẩu PostgreSQL"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=product_api;Username=postgres;Password=$password" --project src/ProductApi/ProductApi.csproj
```

Kiểm tra lại cấu hình:

```powershell
dotnet user-secrets list --project src/ProductApi/ProductApi.csproj
```

### Cập nhật migration

Nếu máy chưa có `dotnet-ef`:

```powershell
dotnet tool install --global dotnet-ef --version 8.*
```

Cập nhật database:

```powershell
dotnet ef database update --project src/ProductApi/ProductApi.csproj --startup-project src/ProductApi/ProductApi.csproj
```

### Build và chạy API

```powershell
dotnet restore ProductApi.sln
dotnet build ProductApi.sln --configuration Release --no-restore
dotnet run --project src/ProductApi/ProductApi.csproj --launch-profile http
```

Sau khi chạy:

- API: `http://localhost:5291`
- Swagger: `http://localhost:5291/swagger`

## Postman regression test

Collection nằm tại:

```text
postman/ProductApi-Week6.postman_collection.json
```

Có thể import file này vào Postman hoặc chạy bằng Newman sau khi API đã khởi động:

```powershell
npx --yes newman run postman/ProductApi-Week6.postman_collection.json
```

Collection tự tạo tên dữ liệu bằng timestamp, lấy ID từ response và lưu vào collection variables. Vì API chỉ có soft delete nên bước cleanup sẽ đánh dấu dữ liệu test là đã xóa thay vì xóa cứng khỏi PostgreSQL.

Kết quả chạy gần nhất:

| Hạng mục         | Đã chạy | Thất bại |
| ------------------ | ---------: | ---------: |
| Request            |         37 |          0 |
| Test script        |         37 |          0 |
| Pre-request script |         37 |          0 |
| Assertion          |         51 |          0 |

Checklist chi tiết nằm trong `postman/REGRESSION_CHECKLIST.md`.

## Nội dung đã làm trong tuần 6

### Day 1 - Response và exception

- Tạo response dùng chung cho API.
- Thêm custom exception cho lỗi `400`, `404` và `409`.
- Xử lý exception tập trung bằng middleware.
- Chuẩn hóa response validation.
- Không trả stack trace cho client.

### Day 2 - Product list

- Thêm search, filter, sort và pagination.
- Tạo DTO cho query parameter.
- Tạo response phân trang dùng chung.
- Giới hạn field được phép sort và giá trị `pageSize`.

### Day 3 - Category

- Thêm Category API theo Controller, Service và Repository.
- Thiết lập quan hệ một-nhiều giữa Category và Product.
- Kiểm tra Category khi thêm hoặc sửa Product.
- Không cho xóa Category nếu còn Product sử dụng.
- Trả thêm `categoryName` trong Product response.

### Day 4 - Audit và soft delete

- Thêm `CreatedAt`, `UpdatedAt`, `DeletedAt` và `IsDeleted`.
- Tự cập nhật audit fields trong `AppDbContext`.
- Chuyển Product và Category sang soft delete.
- Thêm partial unique index cho dữ liệu đang hoạt động.
- Thêm endpoint restore Product.
- Ghi log request và lỗi nghiệp vụ.

### Day 5 - Review và regression

- Review lại Product, Category, DTO, service, repository và middleware.
- Tách phần chuẩn hóa khoảng trắng để dùng chung.
- Validation độ dài tên sau khi normalize.
- Giảm code lặp khi lưu Product và xử lý unique constraint.
- Tạo Postman collection cho toàn bộ flow chính của tuần 6.
- Chạy Newman với 37 request và 51 assertion đều pass.

## Một số ghi chú sau khi làm

- Validation nên kiểm tra đúng dữ liệu sẽ được lưu. Ví dụ chuỗi `" A     "` có nhiều ký tự nhưng sau khi gom khoảng trắng chỉ còn `"A"`.
- Kiểm tra duplicate ở service giúp trả lỗi dễ hiểu, còn unique index trong database là lớp bảo vệ cuối khi có nhiều request chạy đồng thời.
- Soft delete giúp giữ lịch sử nhưng mọi truy vấn đọc, kiểm tra trùng và kiểm tra quan hệ đều phải bỏ qua bản ghi đã xóa.
- Restore không chỉ đổi `IsDeleted` về `false`; cần kiểm tra lại Category và tên trùng trước khi lưu.
- Không nên dùng ID cố định trong regression test. Lấy ID từ response giúp collection chạy lại dễ hơn.
- `IQueryable` phù hợp để ghép search, filter và sort trước khi EF Core tạo câu SQL.

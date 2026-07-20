# Advanced Product API

Đây là project Product API của tuần 6, được làm tiếp từ project tuần trước.

Trong Day 1 mình tập trung vào việc chuẩn hóa response và xử lý exception chung cho toàn bộ API. Sang Day 2 mình làm phần lấy danh sách sản phẩm với tìm kiếm, lọc, sắp xếp và phân trang. Đến Day 3 mình bổ sung Category API và thiết lập quan hệ giữa Category với Product. Trong Day 4 mình triển khai audit fields, soft delete, khôi phục Product đã xóa và logging cơ bản cho mỗi request.

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

## Những phần đã làm trong Day 2

- Tạo `ProductQueryDto` để gom các query parameter của API danh sách.
- Tạo `PagedResult<T>` dùng chung cho dữ liệu phân trang.
- Tìm kiếm sản phẩm theo tên, không phân biệt chữ hoa chữ thường.
- Lọc sản phẩm theo `categoryId`.
- Sắp xếp theo `id`, `name`, `price` hoặc `quantity`.
- Hỗ trợ thứ tự `asc` và `desc`.
- Giới hạn `pageSize` từ 1 đến 100.
- Dùng chung điều kiện tìm kiếm và lọc khi lấy danh sách và đếm tổng số bản ghi.
- Sắp xếp trước khi dùng `Skip` và `Take` để kết quả giữa các trang ổn định.

## Những phần đã làm trong Day 3

- Tạo Category API theo cấu trúc Controller, Service và Repository.
- Hỗ trợ đầy đủ các thao tác lấy danh sách, lấy theo ID, thêm, cập nhật và xóa Category.
- Thiết lập quan hệ một-nhiều giữa Category và Product.
- Tạo migration cho bảng Categories, dữ liệu mẫu và foreign key.
- Kiểm tra Category tồn tại trước khi thêm hoặc cập nhật Product.
- Không cho xóa Category nếu vẫn còn Product sử dụng Category đó.
- Bổ sung `categoryName` trong response của Product.
- Dùng `Include` để lấy Category khi đọc Product.

## Những phần đã làm trong Day 4

- Tạo `AuditableEntity` dùng chung cho Product và Category với các field `CreatedAt`, `UpdatedAt`, `DeletedAt` và `IsDeleted`.
- Tự động gán audit fields trong `AppDbContext` trước khi lưu thay đổi.
- Chuyển thao tác DELETE Product và Category từ xóa cứng sang soft delete.
- Lọc thủ công các Product và Category đã xóa khỏi API danh sách, chi tiết và các truy vấn kiểm tra dữ liệu.
- Chuyển unique index sang partial unique index để bản ghi đã xóa không chặn việc tạo dữ liệu mới cùng tên.
- Tạo endpoint `PATCH /api/products/{id}/restore` để khôi phục Product đã xóa.
- Kiểm tra Category và dữ liệu trùng trước khi khôi phục Product.
- Tạo `RequestLoggingMiddleware` để ghi method, path, status code và thời gian xử lý request ra console.
- Kết hợp request logging với `GlobalExceptionMiddleware` để ghi warning cho lỗi nghiệp vụ và error cho lỗi server.

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

Response danh sách sản phẩm có thêm thông tin phân trang:

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
  "timestampUtc": "2026-07-17T00:00:00Z"
}
```

## Query parameter của API danh sách

| Tham số | Mặc định | Ghi chú |
| --- | --- | --- |
| `search` | Không có | Tìm theo tên sản phẩm |
| `categoryId` | Không có | Nếu truyền vào thì phải lớn hơn 0 |
| `page` | `1` | Phải lớn hơn hoặc bằng 1 |
| `pageSize` | `10` | Nhận giá trị từ 1 đến 100 |
| `sortBy` | `id` | Nhận `id`, `name`, `price`, `quantity` |
| `sortOrder` | `asc` | Nhận `asc` hoặc `desc` |

Ví dụ:

```http
GET /api/products?search=phone&categoryId=1&sortBy=price&sortOrder=desc&page=1&pageSize=10
```

Response Product trong Day 3 có thêm tên Category:

```json
{
  "id": 1,
  "name": "Laptop",
  "categoryId": 1,
  "categoryName": "Electronics",
  "price": 1500.00,
  "quantity": 10
}
```

## Các endpoint

| Method | Endpoint | Chức năng |
| --- | --- | --- |
| `GET` | `/api/products` | Lấy danh sách, tìm kiếm, lọc, sắp xếp và phân trang các Product chưa xóa |
| `GET` | `/api/products/{id}` | Lấy Product chưa xóa theo ID |
| `POST` | `/api/products` | Thêm Product |
| `PUT` | `/api/products/{id}` | Cập nhật Product |
| `PATCH` | `/api/products/{id}/restore` | Khôi phục Product đã soft delete |
| `DELETE` | `/api/products/{id}` | Soft delete Product |
| `GET` | `/api/categories` | Lấy danh sách Category chưa xóa |
| `GET` | `/api/categories/{id}` | Lấy Category chưa xóa theo ID |
| `POST` | `/api/categories` | Thêm Category |
| `PUT` | `/api/categories/{id}` | Cập nhật Category |
| `DELETE` | `/api/categories/{id}` | Soft delete Category nếu không còn Product đang hoạt động |
| `GET` | `/api/health` | Kiểm tra API có đang chạy không |
| `GET` | `/api/info` | Xem thông tin project |

## Các status code đang xử lý

| Status                        | Trường hợp                                   |
| ----------------------------- | ----------------------------------------------- |
| `200 OK`                    | Lấy, cập nhật hoặc khôi phục dữ liệu thành công |
| `201 Created`               | Tạo Product hoặc Category thành công       |
| `204 No Content`            | Soft delete Product hoặc Category thành công |
| `400 Bad Request`           | Dữ liệu không hợp lệ hoặc Category không tồn tại |
| `404 Not Found`             | Không tìm thấy dữ liệu đang hoạt động hoặc Product đã xóa cần khôi phục |
| `409 Conflict`              | Dữ liệu bị trùng, Category vẫn còn Product hoặc không thể khôi phục Product |
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

## Kết quả test Day 2

| Trường hợp test | Kết quả |
| --- | --- |
| Không truyền query parameter | Dùng `page = 1`, `pageSize = 10`, `sortBy = id`, `sortOrder = asc` |
| Phân trang với `pageSize = 2` | Trả đúng số item và tổng số trang |
| `page = 0` | `400 Bad Request` |
| `pageSize = 101` | `400 Bad Request` |
| Lọc theo category có dữ liệu | Trả đúng sản phẩm và `totalItems` |
| Lọc theo category không có dữ liệu | Trả danh sách rỗng, không trả `404` |
| Kết hợp search và category | Chỉ trả sản phẩm thỏa cả hai điều kiện |
| Sắp xếp theo giá tăng và giảm | Thứ tự sản phẩm đúng |
| Sắp xếp theo tên tăng dần | Thứ tự tên đúng |
| Sắp xếp theo số lượng giảm dần | Thứ tự số lượng đúng |
| `sortBy` không hợp lệ | `400 Bad Request` |
| `sortOrder` không hợp lệ | `400 Bad Request` |
| Sắp xếp kết hợp phân trang | Sắp xếp trước rồi mới lấy dữ liệu của trang |

## Kết quả test Day 3

| Trường hợp test | Kết quả |
| --- | --- |
| GET danh sách Category | `200 OK` |
| GET Category theo ID | `200 OK` hoặc `404 Not Found` |
| POST Category hợp lệ | `201 Created` |
| POST tên Category không hợp lệ | `400 Bad Request` |
| POST tên Category bị trùng | `409 Conflict` |
| PUT Category hợp lệ | `200 OK` |
| DELETE Category không có Product | `204 No Content` |
| DELETE Category đang có Product | `409 Conflict` |
| POST Product với Category không tồn tại | `400 Bad Request` |
| PUT Product với Category không tồn tại | `400 Bad Request` |
| GET danh sách và chi tiết Product | Có `categoryName` đúng với `categoryId` |
| POST và PUT Product hợp lệ | Response có `categoryName` đúng |

## Ghi chú Day 3

Một số điểm mình rút ra trong lúc làm:

- `CategoryId` là foreign key, còn `Category` là navigation property để truy cập dữ liệu liên quan.
- Mình kiểm tra Category tồn tại ở service để trả lỗi dễ hiểu, đồng thời vẫn giữ foreign key ở database làm lớp bảo vệ cuối.
- Khi xóa Category, mình kiểm tra Product liên quan trước để trả `409 Conflict` thay vì để lỗi database trở thành `500`.
- `DeleteBehavior.Restrict` giúp database không tự động xóa Product khi Category bị xóa.
- `Include` giúp EF Core lấy Product và Category trong cùng truy vấn để map `categoryName` vào response.
- DTO giúp API chỉ nhận và trả những field cần thiết, không trả trực tiếp entity của EF Core.

## Kết quả test Day 4

| Trường hợp test | Kết quả |
| --- | --- |
| Build solution | Thành công, không có warning hoặc error |
| Kiểm tra migration | Tất cả migration đã được áp dụng |
| GET danh sách Product | `200 OK`, không có Product đã soft delete |
| GET Product đã soft delete | `404 Not Found` |
| DELETE Product | `204 No Content`, dữ liệu vẫn còn trong database với `IsDeleted = true` |
| PATCH khôi phục Product đã xóa | `200 OK` |
| GET Product sau khi khôi phục | `200 OK` |
| PATCH Product đang hoạt động | `404 Not Found` |
| PATCH Product khi Category đã xóa | `409 Conflict` |
| PATCH Product bị trùng với Product đang hoạt động | `409 Conflict` |
| DELETE Category còn Product đang hoạt động | `409 Conflict` |
| Request thành công | Console có log bắt đầu, kết thúc, status code và thời gian xử lý |
| Request lỗi `404` hoặc `409` | Console có warning và log status code cuối cùng |
| Kiểm tra lại CRUD Product | POST `201`, PUT `200`, DELETE `204`, restore `200` |

## Ghi chú Day 4

Một số điểm mình rút ra trong lúc làm:

- Soft delete giúp giữ lại lịch sử dữ liệu và cho phép khôi phục, nhưng mọi truy vấn đọc và kiểm tra trùng phải chủ động bỏ qua bản ghi đã xóa.
- Việc gán `CreatedAt` và `UpdatedAt` trong `AppDbContext` giúp các repository và service không phải lặp lại logic thời gian.
- `CreatedAt` chỉ được gán khi tạo mới, còn `UpdatedAt` được cập nhật mỗi khi entity thay đổi, bao gồm cả lúc soft delete và restore.
- Partial unique index chỉ áp dụng cho bản ghi có `IsDeleted = false`, nhờ đó có thể tạo lại dữ liệu cùng tên sau khi bản ghi cũ đã bị xóa mềm.
- Restore không chỉ đổi `IsDeleted` về `false`; service còn phải kiểm tra Category gốc và dữ liệu trùng để tránh khôi phục entity về trạng thái không hợp lệ.
- Đặt `RequestLoggingMiddleware` trước `GlobalExceptionMiddleware` giúp middleware logging đo được toàn bộ request và ghi đúng status code sau khi exception đã được chuyển thành response.
- `ILogger` hỗ trợ structured logging bằng message template, giúp log rõ ràng và dễ tìm kiếm hơn so với dùng `Console.WriteLine`.

## Ghi chú Day 2

Một số điểm mình rút ra trong lúc làm:

- `IQueryable` giúp ghép từng điều kiện search và filter trước khi EF Core chạy câu SQL.
- Query lấy danh sách và query đếm tổng số bản ghi phải dùng cùng điều kiện, nếu không metadata phân trang sẽ bị sai.
- Không nên nhận tùy ý tên property từ client để sort. Mình dùng danh sách field cho phép và `switch` để kiểm soát.
- Khi nhiều sản phẩm có cùng giá, tên hoặc số lượng, mình sort thêm theo `Id` để kết quả phân trang ổn định hơn.
- Danh sách không có kết quả vẫn là request hợp lệ nên trả `200 OK` với `items` rỗng.

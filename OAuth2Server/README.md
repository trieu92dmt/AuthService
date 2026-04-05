# Run the following commands to create the database and apply the initial migration:
dotnet ef migrations add Init --project Infracstructure --startup-project OAuth2Server

# ASP.NET Identity
dotnet ef migrations add InitIdentity --project Infracstructure --startup-project OAuth2Server

# Update database
dotnet ef database update Init --project Infracstructure --startup-project OAuth2Server

# Ở levlel production cần tạo mới certificate để đảm an toàn
# Cách 1: Dùng file .pfx (phổ biến nhất)
	- Bước 1: Tạo certificate (.pfx)
	- Bước 2: Đặt file vào project
	- Bước 3: Load vào OpenIddict
# Cách 2: Dùng Windows Certificate Store
	- Bước 1: Tạo certificate (.pfx)
	- Bước 2: Cài đặt certificate vào Windows Certificate Store
	- Bước 3: Lấy thumbprint của certificate
	- Bước 4: Cấu hình IdentityServer để sử dụng certificate từ Windows Certificate Store
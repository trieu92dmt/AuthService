# Link image trên docker hub: 
# https://hub.docker.com/r/microsoft/dotnet-sdk
# AS build: Đánh dấu giai đoạn build (Multi-stage Build)
# Tách biệt giai đoạn build và runtime, có thể tham chiếu bằng cách dùng tên build

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build

#Chuyển đến thư mục /src (Tự động tạo mới nếu chưa tồn tại)

WORKDIR /src

# Copy file project vào container
# Copy những file có đuôi .csproj
# ./: Copy đến thư mục container hiện tại (Được tạo bởi WORKDIR trước đó)

COPY *.csproj ./

# Restore các gói NuGet từ file csproj

RUN dotnet restore

# Copy toàn bộ mã nguồn và build ứng dụng
# Nguyên nhân dùng COPY *.csproj ./ trước rồi mới copy toàn bộ mã nguồn 
# Nhằm tận dụng Docker cache khi source thay đổi nhưng nuget không đổi

COPY . ./
RUN dotnet publish -c Release -o /app

# Sử dụng hình ảnh .NET Runtime để chạy ứng dụng

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime

#Đổi thư mục làm việc từ /src sang /app

WORKDIR /app

# Copy ứng dụng đã build từ bước trước
# Copy thư mục /app từ giai đoạn build hiện tại
# Lưu ý có 2 thư mục /app khác nhau (giai đoạn build và runtime)

COPY --from=build /app .

# Mở cổng để container lắng nghe (thường là 80 hoặc 5000)
# Lưu ý: lệnh expose chỉ để thông báo cổng mở, cần ánh xạ khi run container

EXPOSE 80
EXPOSE 5000

# Lệnh khởi chạy ứng dụng
ENTRYPOINT ["dotnet", "Authentication.dll"]
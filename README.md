# EndguinField Project Powered by ASP .Net MVC 
### Stack
- Frontend: Pure HTML/CSS/JS
- Backend: .NET MVC C#
- Database: MongoDB Atlas
- Object Storage: Amazon S3

### Setup MVC Project
Install .NET in your PC.
```
https://dotnet.microsoft.com/en-us/download
```

### Install Dependencies
You can see all dependency at EndguinField.csproj
```
dotnet restore
```

### Don't forget to add .env file
Contact xSistted for .env

### Running Dev
```
dotnet watch
```

---

## Deploy with Docker + Nginx Proxy Manager (NPM) + Cloudflare

### Build & run the app container on your EC2

Create a shared Docker network (must match what NPM uses):
```
docker network create proxy-net
```

Then run this app:
```
docker-compose.yml up -d --build
```
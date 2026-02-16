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

### 1) Build & run the app container on your EC2

Create a shared Docker network (must match what NPM uses):
```
docker network create npm_proxy
```

Then run this app:
```
docker compose -f docker-compose.app.yml up -d --build
```

The app listens on container port `8080` and is meant to be accessed via NPM.

### 2) Configure NPM

In the NPM UI:
- Add **Proxy Host**
- Domain: `yourdomain.com`
- Forward Hostname: `endguinfield`
- Forward Port: `8080`
- Enable **Websockets** (optional)
- SSL: Request a **Let's Encrypt** certificate + enable **Force SSL**

### 3) Configure Cloudflare DNS

In Cloudflare DNS:
- Create an `A` record for `yourdomain.com` pointing to your EC2 public IPv4
- Ensure EC2 Security Group allows inbound `80` and `443`

If Let's Encrypt fails while Cloudflare proxy is enabled, temporarily set the record to **DNS only** (grey cloud) to issue the cert, or use a DNS challenge in NPM.
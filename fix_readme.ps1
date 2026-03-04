$content = @"
# EndguinField (Web App)

โปรเจกต์นี้พัฒนาด้วย **ASP.NET Core MVC** โดยมีระบบฐานข้อมูลเป็น **MongoDB Atlas** และใช้ **Amazon S3** สำหรับจัดการไฟล์รูปภาพ/สื่อต่างๆ

---

## 🛠 Tech Stack
- **Frontend:** HTML, CSS, JavaScript (Razor Views)
- **Backend:** ASP.NET Core MVC (C#)
- **Database:** MongoDB Atlas
- **Object Storage:** Amazon S3
- **Deployment:** Docker + Nginx Proxy Manager (NPM) + Cloudflare

---

## 📂 โครงสร้างโปรเจกต์ (Project Structure)
สำหรับคนที่เข้ามาดูโค้ดครั้งแรก โครงสร้างของโปรเจกต์จะแบ่งตามสถาปัตยกรรม MVC (Model-View-Controller) ดังนี้:

- **`Controllers/`**: ส่วนควบคุมการทำงานและรับ Request จากผู้ใช้ (Routing & HTTP Handling)
  - `HomeController.cs`: จัดการหน้าแรก, การ Login, Privacy
  - `ProfileController.cs`: จัดการหน้าโปรไฟล์ผู้ใช้
  - `EventDetailController.cs`: จัดการหน้ารายละเอียดของกิจกรรม (Event)
- **`Models/`**: โครงสร้างข้อมูล (Schema) และ ViewModels ที่ใช้ส่งข้อมูลไปยังหน้า View
  - `Class/`: คลาสหลักของระบบที่แมปกับ Database เช่น `User`, `Event`, `Comment`, `FriendRequest`, `Participation`, `Tag`
  - `ViewModels/`: คลาสที่รวมข้อมูลหลายๆ ส่วนเพื่อส่งไปแสดงผลที่หน้า View (เช่น `EventDetailViewModel`, `ProfileViewModel`)
- **`Services/`**: ส่วนจัดการ Business Logic และการเชื่อมต่อภายนอก (แยกออกจาก Controller เพื่อให้โค้ดสะอาดและนำไปใช้ซ้ำได้)
  - `UserService.cs`: จัดการข้อมูลและ Logic ที่เกี่ยวกับผู้ใช้
  - `MongoDB/`: จัดการการเชื่อมต่อและ Query ข้อมูลจาก MongoDB (`MongoService.cs`, `TagSchemaServices.cs`)
  - `S3/`: จัดการการอัปโหลด/ดาวน์โหลดไฟล์กับ Amazon S3 (`S3Service.cs`)
- **`Views/`**: หน้าจอแสดงผล (UI) เขียนด้วย HTML ผสม C# (Razor `.cshtml`)
  - แบ่งโฟลเดอร์ตาม Controller เช่น `Home/`, `Profile/`, `EventDetail/`
  - `Shared/`: เก็บ Component ที่ใช้ซ้ำๆ เช่น Navbar (`_Layout.cshtml`), การ์ดกิจกรรม (`_EventCard.cshtml`), รายชื่อเพื่อน (`_FriendList.cshtml`)
- **`wwwroot/`**: เก็บไฟล์ Static เช่น CSS, JavaScript, รูปภาพ (Assets) และ Library ภายนอก (Bootstrap, FontAwesome)

---

## 🔄 Workflow การทำงานของระบบ (Data Flow)
เมื่อผู้ใช้เข้ามาใช้งานระบบ การทำงานจะเป็นไปตามลำดับนี้:
1. **Client (Browser)** ส่ง Request มาที่ URL (เช่น `/Profile/Index`)
2. **Controller** (`ProfileController`) รับ Request และตรวจสอบสิทธิ์/ข้อมูลเบื้องต้น
3. **Controller** เรียกใช้งาน **Service** (`UserService`, `MongoService`) เพื่อดึงข้อมูลจาก **MongoDB** หรือไฟล์จาก **S3**
4. **Service** ทำการประมวลผล Business Logic และส่งข้อมูลกลับมาให้ **Controller**
5. **Controller** นำข้อมูลไปใส่ใน **ViewModel** (`ProfileViewModel`) และส่งต่อไปที่ **View**
6. **View** (`Views/Profile/Index.cshtml`) นำข้อมูลไป Render เป็น HTML และส่งกลับไปแสดงผลที่ Browser

---

## 🚀 การติดตั้งและรันโปรเจกต์ (สำหรับ Developer)

### 1. สิ่งที่ต้องมี (Prerequisites)
- ติดตั้ง [.NET SDK](https://dotnet.microsoft.com/en-us/download)
- ข้อมูลการเชื่อมต่อ MongoDB (Connection String)
- ข้อมูลการเชื่อมต่อ Amazon S3 (Access Key, Secret Key, Bucket Name)

### 2. การตั้งค่า Environment
โปรเจกต์นี้ต้องการไฟล์ตั้งค่า (Environment Variables) ติดต่อทีม (xSistted) เพื่อขอไฟล์ `.env` หรือตั้งค่าใน `appsettings.Development.json` ดังนี้:
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb+srv://<username>:<password>@cluster.mongodb.net/",
    "DatabaseName": "EndguinField"
  },
  "AWS": {
    "AccessKey": "YOUR_ACCESS_KEY",
    "SecretKey": "YOUR_SECRET_KEY",
    "BucketName": "YOUR_BUCKET_NAME"
  }
}
```

### 3. การรันโปรเจกต์ (Development)
เปิด Terminal ในโฟลเดอร์โปรเจกต์แล้วรันคำสั่ง:
```bash
# ติดตั้ง Dependencies
dotnet restore

# รันโปรเจกต์แบบ Hot Reload (แก้โค้ดแล้วเว็บอัปเดตทันที)
dotnet watch
```

---

## 🐳 การ Deploy ด้วย Docker (Production)
โปรเจกต์นี้รองรับการ Deploy ผ่าน Docker ร่วมกับ Nginx Proxy Manager (NPM) และ Cloudflare

1. สร้าง Docker Network (ให้ตรงกับที่ NPM ใช้):
   ```bash
   docker network create proxy-net
   ```
2. Build และ Run Container:
   ```bash
   docker-compose up -d --build
   ```
   *(แอปจะรันที่พอร์ต `8080` ภายใน Container)*
3. ตั้งค่า **Nginx Proxy Manager**:
   - Domain: `yourdomain.com`
   - Forward Hostname: `endguinfield`
   - Forward Port: `8080`
   - เปิดใช้งาน SSL (Let's Encrypt)
4. ตั้งค่า **Cloudflare DNS**:
   - ชี้ A Record ไปที่ IP ของ Server
"@
[System.IO.File]::WriteAllText("README.md", $content, [System.Text.Encoding]::UTF8)

$git = "C:\Program Files\Git\cmd\git.exe"
& $git add README.md
& $git commit -m 'Fix README encoding and update content'
& $git push -u origin main --force

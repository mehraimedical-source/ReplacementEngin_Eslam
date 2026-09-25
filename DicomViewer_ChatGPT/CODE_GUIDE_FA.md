# راهنمای درک کد DicomViewer_ChatGPT

این فایل برای توضیح معماری پروژه به زبان ساده است. هدف این است که هنگام بروز مشکل بتوان مسیر داده را دنبال کرد و محل مناسب Debug را سریع‌تر پیدا کرد. با اضافه شدن قابلیت‌های جدید، این فایل نیز باید به‌روز شود.

## 1. مسیر کلی داده

مسیر اصلی برنامه به شکل زیر است:

DICOM Files
→ DicomSeriesLoader
→ ProcessedDicomImage[]
→ MedicalDicomViewerControl
→ MPR (Axial / Sagittal / Coronal)
→ CpuVolumeRenderer (3D)

بهتر است هنگام Debug همیشه ابتدا مشخص کنیم مشکل در کدام مرحله از این زنجیره ایجاد شده است.

## 2. فایل‌های اصلی

### Form1.cs و Form1.Designer.cs
فرم آزمایشی برنامه هستند. انتخاب/بارگذاری DICOM و Toolbar در این قسمت قرار دارند.

نکته مهم: منطق پردازش پزشکی نباید داخل Form1 قرار بگیرد. فرم فقط باید دستورات کاربر را به MedicalDicomViewerControl منتقل کند.

### ProcessedDicomImage.cs
مدل داخلی یک Slice پردازش‌شده است.

اطلاعات مهم آن شامل:
- Gray8: پیکسل‌های 8 بیتی برای نمایش سریع.
- Modality16: داده با دقت بیشتر، مخصوصاً HU در CT در صورت قابل دسترس بودن.
- ImagePositionPatient: موقعیت Slice در فضای بیمار.
- ImageOrientationPatient: جهت Row و Column تصویر در فضای بیمار.
- PixelSpacing: اندازه واقعی هر Pixel بر حسب میلی‌متر.

اگر ابعاد، جهت یا موقعیت تصاویر اشتباه باشد، این فایل و مرحله ساخت آن از اولین نقاط بررسی هستند.

### DicomSeriesLoader.cs
وظیفه خواندن فایل‌های DICOM را دارد.

کارهای اصلی:
1. جستجوی فایل‌ها.
2. تشخیص فایل‌هایی که PixelData دارند.
3. گروه‌بندی بر اساس SeriesInstanceUID.
4. انتخاب بزرگ‌ترین Series تصویری.
5. مرتب کردن Sliceها.
6. Decode کردن PixelData.
7. استخراج اطلاعات هندسی DICOM.
8. تولید ProcessedDicomImage[].

برای پایداری، مسیر RenderImage/AsClonedBitmap به عنوان fallback نگه داشته شده است. حذف این fallback بدون تست Datasetهای مختلف توصیه نمی‌شود.

### MedicalDicomViewerControl.cs
مرکز اصلی Viewer است و در حال حاضر حساس‌ترین فایل پروژه محسوب می‌شود.

وظایف مهم:
- دریافت Volume با Active(...).
- محاسبه spacing.
- آماده‌سازی Volume سریع.
- ساخت سه نمای MPR.
- نگهداری نقطه مشترک crosshairPatient.
- مدیریت صفحات Axial/Coronal/Sagittal.
- حرکت Crosshair.
- چرخش خطوط و Oblique MPR.
- ارتباط با CpuVolumeRenderer.
- Reset Axes و Reset 3D.

به دلیل وابستگی رفتارهای MPR به یکدیگر، تغییر این فایل باید کوچک و مرحله‌ای باشد.

### MedicalDicomViewerControl.Designer.cs
فقط چیدمان کنترل‌های خود UserControl را نگهداری می‌کند.

منطق پردازش تصویر نباید در Designer نوشته شود.

### CpuVolumeRenderer.cs
Renderer سه‌بعدی CPU است و وابستگی GPU ندارد.

ورودی آن Volume و spacing است. برای CT، در صورت وجود Modality16 از داده HU استفاده می‌شود. Rotate و Zoom وضعیت Camera را تغییر می‌دهند و Render تصویر سه‌بعدی جدید تولید می‌کند.

Reset3D فقط Camera سه‌بعدی را Reset می‌کند و نباید MPR را تغییر دهد.

## 3. مفاهیم مهم MPR

### Voxel Space
مختصات داخل آرایه Volume است:
X = ستون
Y = ردیف
Z = شماره Slice

### Patient Space
مختصات واقعی بیمار بر حسب میلی‌متر است. MPR اصلی پروژه بر پایه این فضا ساخته می‌شود تا Datasetهایی که Orientation متفاوت دارند درست نمایش داده شوند.

### سه Plane
سه صفحه اصلی داریم:
- axialPlane
- coronalPlane
- sagittalPlane

هر Plane دارای بردارهای U و V برای محورهای داخل صفحه و N برای Normal صفحه است.

### crosshairPatient
نقطه مشترک سه صفحه MPR در Patient Space است.

حرکت مرکز خطوط باید این نقطه را تغییر دهد و نماهای مرتبط را از محل جدید Reslice کند.

### Display Origin
برای هر View یک Display Origin جدا نگهداری می‌شود. این موضوع مخصوصاً هنگام Drag و Rotation مهم است تا تصویر منبع ناگهان جابه‌جا نشود.

اگر بعد از Move → Rotate → Move تصویر یا خطوط Jump کردند، بخش‌های crosshairPatient، Display Origin و منطق Drag باید بررسی شوند.

## 4. روند ساخت یک نمای MPR

به صورت ساده:

Plane + Patient-space center
→ تعیین اندازه فیزیکی خروجی
→ حرکت روی Pixelهای تصویر خروجی
→ تبدیل هر نقطه Patient Space به مختصات Volume
→ Trilinear Sampling
→ تولید Bitmap
→ رسم خطوط Cross-reference

SamplePatientFast برای سرعت اهمیت زیادی دارد. تغییر آن می‌تواند هم کیفیت و هم Performance هر سه MPR را تحت تأثیر قرار دهد.

## 5. روند 3D

ProcessedDicomImage[]
→ ساخت Volume پیوسته
→ SetCtVolume برای CT/HU یا SetVolume برای fallback
→ CpuVolumeRenderer.Render(...)
→ Bitmap
→ PictureBox سه‌بعدی

Mouse Drag دوربین را می‌چرخاند.
Mouse Wheel Zoom را تغییر می‌دهد.
Reset 3D دوربین را به وضعیت اولیه برمی‌گرداند.

Renderer فعلی CPU-based است؛ بنابراین کیفیت، تعداد Sampleها و Resolution مستقیماً روی سرعت اثر دارند.

## 6. هنگام بروز مشکل از کجا شروع کنیم؟

اگر DICOM Load نمی‌شود:
DicomSeriesLoader را بررسی کنید.

اگر Sliceها ترتیب اشتباه دارند:
ImagePositionPatient، ImageOrientationPatient و ترتیب Series را بررسی کنید.

اگر تصویر کشیده یا فشرده است:
PixelSpacing و spacingZ را بررسی کنید.

اگر جهت Axial/Coronal/Sagittal اشتباه است:
Patient Space و Planeها را بررسی کنید.

اگر Crosshair یا خطوط Jump می‌کنند:
crosshairPatient، Display Origin و MouseDown/MouseMove مربوط به MPR را بررسی کنید.

اگر MPR کند شده:
BuildPlane و SamplePatientFast اولین نقاط بررسی هستند.

اگر فقط 3D مشکل دارد:
ابتدا CpuVolumeRenderer را بررسی کنید؛ تا حد امکان MPR را تغییر ندهید.

اگر Reset Axes مشکل دارد:
MedicalDicomViewerControl.ResetAxes را بررسی کنید.

اگر Reset 3D مشکل دارد:
MedicalDicomViewerControl.Reset3D و CpuVolumeRenderer.ResetCamera را بررسی کنید.

## 7. قانون تغییر کد

برای کاهش Regression:
1. قبل از تغییر، نسخه فعلی main بررسی شود.
2. هر بار فقط یک رفتار مشخص تغییر کند.
3. بخش‌های سالم MPR/3D بدون دلیل تغییر نکنند.
4. بعد از تغییر با Datasetهای شناخته‌شده تست شود.
5. تغییرات با Git commit کوچک نگهداری شوند.
6. Commentهای جدید تا حد امکان فارسی و توضیح‌دهنده «چرا» باشند، نه فقط «چه کاری».

## 8. Dependencyها

پروژه روی .NET Framework 4.8 و x64 است.

Dependency خارجی اصلی:
fo-dicom.Desktop 4.0.8

3D فعلی کتابخانه خارجی جداگانه ندارد و CpuVolumeRenderer کد خود پروژه است.

## 9. ترتیب پیشنهادی برای مطالعه پروژه

برای درک کد بهتر است به این ترتیب مطالعه شود:

1. ProcessedDicomImage.cs
2. DicomSeriesLoader.cs
3. MedicalDicomViewerControl.Designer.cs
4. MedicalDicomViewerControl.cs — ابتدا Active و آماده‌سازی Volume
5. بخش Patient Space و MPR
6. بخش Mouse interaction و Crosshair
7. CpuVolumeRenderer.cs
8. Form1.cs و Toolbar

این ترتیب کمک می‌کند ابتدا ساختار داده را بفهمید و سپس وارد الگوریتم‌های پیچیده‌تر شوید.

---
این سند باید همراه پروژه رشد کند. هر قابلیت مهم جدید باید یک توضیح کوتاه در همین فایل داشته باشد تا دانش پروژه فقط داخل کد یا تاریخچه گفتگو باقی نماند.

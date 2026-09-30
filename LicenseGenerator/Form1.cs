using System;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace LicenseGenerator
{
    public partial class Form1 : Form
    {
        private byte[] secretKey;
        private readonly string keyPath = Path.Combine(
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "LicenseGenerator"), "issuer-key.dat");

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(keyPath));
                if (!File.Exists(keyPath))
                {
                    byte[] generated = LicenseCodec.GenerateSecretKey();
                    try
                    {
                        byte[] protectedKey = ProtectKey(generated);
                        // CreateNew prevents silently replacing another instance's key.
                        using (FileStream stream = new FileStream(keyPath, FileMode.CreateNew, FileAccess.Write))
                            stream.Write(protectedKey, 0, protectedKey.Length);
                    }
                    finally { Array.Clear(generated, 0, generated.Length); }
                }
                SetKey(ReadKey(keyPath));
                ShowStatus("کلید آماده است. اطلاعات را وارد کنید یا کد را Decode کنید.", false);
            }
            catch (Exception ex)
            {
                ShowError("بارگذاری کلید ناموفق بود. از Load Key استفاده کنید.", ex);
            }
        }

        private static byte[] ProtectKey(byte[] key)
        {
            return ProtectedData.Protect(key, null, DataProtectionScope.CurrentUser);
        }

        private static byte[] ReadKey(string path)
        {
            FileInfo file = new FileInfo(path);
            if (file.Length < 1 || file.Length > 4096)
                throw new FormatException("Invalid key file.");
            byte[] key = ProtectedData.Unprotect(File.ReadAllBytes(path), null, DataProtectionScope.CurrentUser);
            if (key.Length != 32)
            {
                Array.Clear(key, 0, key.Length);
                throw new FormatException("Expected a 32-byte key.");
            }
            return key;
        }

        private void SetKey(byte[] key)
        {
            if (secretKey != null) Array.Clear(secretKey, 0, secretKey.Length);
            secretKey = key;
            btnCode.Enabled = true;
            btnDecode.Enabled = true;
            btnSaveKey.Enabled = true;
            lblKeyStatus.Text = "کلید آماده و ذخیره شده است";
            txtDecodedProduct.Clear();
            txtDecodedCout.Clear();
        }

        private void btnCode_Click(object sender, EventArgs e)
        {
            try
            {
                LicenseInfo info = new LicenseInfo
                {
                    ProductID = (int)numProductID.Value,
                    Cout = (int)numCout.Value
                };
                txtLicense.Text = LicenseCodec.Code(info, secretKey);
                ShowStatus("کد ساخته شد. برای بررسی آن Decode را بزنید.", false);
            }
            catch (Exception ex) { ShowError("ساخت کد ناموفق بود.", ex); }
        }

        private void btnDecode_Click(object sender, EventArgs e)
        {
            txtDecodedProduct.Clear();
            txtDecodedCout.Clear();
            try
            {
                // This generator inspects all products; client authorization must enforce product binding.
                LicenseInfo info = LicenseCodec.Decode(txtLicense.Text, secretKey);
                txtDecodedProduct.Text = info.ProductID.ToString();
                txtDecodedCout.Text = info.Cout.ToString();
                ShowStatus("کد معتبر است؛ اطلاعات استخراج شد.", false);
            }
            catch (Exception ex) { ShowError("کد معتبر نیست یا با این کلید ساخته نشده است.", ex); }
        }

        private void txtLicense_TextChanged(object sender, EventArgs e)
        {
            txtDecodedProduct.Clear();
            txtDecodedCout.Clear();
            lblStatus.Text = "";
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (txtLicense.Text.Trim().Length == 0)
            {
                ShowStatus("ابتدا یک کد بسازید یا وارد کنید.", true);
                return;
            }
            try
            {
                Clipboard.SetText(txtLicense.Text);
                ShowStatus("کد کپی شد.", false);
            }
            catch (Exception ex) { ShowError("کپی کردن ناموفق بود.", ex); }
        }

        private void btnSaveKey_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "Protected key (*.dat)|*.dat";
                dialog.FileName = "license-key-backup.dat";
                dialog.Title = "Save Key - قابل بازیابی با همین حساب ویندوز";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    File.WriteAllBytes(dialog.FileName, ProtectKey(secretKey));
                    ShowStatus("نسخه پشتیبان کلید ذخیره شد؛ بازیابی با همین حساب ویندوز.", false);
                }
                catch (Exception ex) { ShowError("ذخیره کلید ناموفق بود.", ex); }
            }
        }

        private void btnLoadKey_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Protected key (*.dat)|*.dat";
                dialog.Title = "Load Key";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                byte[] loaded = null;
                string temporaryPath = null;
                try
                {
                    loaded = ReadKey(dialog.FileName);
                    bool different = secretKey == null;
                    if (secretKey != null)
                        for (int i = 0; i < secretKey.Length; i++)
                            if (secretKey[i] != loaded[i]) different = true;
                    if (different && MessageBox.Show(this,
                        "کلید فعال عوض می‌شود. کدهای قبلی تنها با کلید قبلی قابل بررسی هستند. ادامه می‌دهید؟",
                        "Load Key", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        return;

                    Directory.CreateDirectory(Path.GetDirectoryName(keyPath));
                    temporaryPath = Path.Combine(Path.GetDirectoryName(keyPath), Guid.NewGuid().ToString("N") + ".tmp");
                    File.WriteAllBytes(temporaryPath, ProtectKey(loaded));
                    if (File.Exists(keyPath))
                        File.Replace(temporaryPath, keyPath, null);
                    else
                        File.Move(temporaryPath, keyPath);
                    SetKey(loaded);
                    loaded = null; // Ownership transferred to the form.
                    ShowStatus("کلید بارگذاری و ذخیره شد.", false);
                }
                catch (Exception ex) { ShowError("بارگذاری کلید ناموفق بود.", ex); }
                finally
                {
                    if (loaded != null) Array.Clear(loaded, 0, loaded.Length);
                    if (temporaryPath != null)
                    {
                        try { if (File.Exists(temporaryPath)) File.Delete(temporaryPath); }
                        catch (IOException) { }
                        catch (UnauthorizedAccessException) { }
                    }
                }
            }
        }

        private void ShowStatus(string message, bool error)
        {
            lblStatus.ForeColor = error ? Color.Firebrick : Color.DarkGreen;
            lblStatus.Text = message;
        }

        private void ShowError(string message, Exception ex)
        {
            ShowStatus(message, true);
            MessageBox.Show(this, message + Environment.NewLine + ex.Message,
                "License Generator", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace WindowsFormsApp14
{
    public partial class Form1 : Form
    {
        private string filePath = "passwords.json";
        private Dictionary<string, string> accounts = new Dictionary<string, string>();

        public Form1()
        {
            InitializeComponent();
            LoadDataFromFile();
            listBoxAccounts.SelectedIndexChanged += listBoxAccounts_SelectedIndexChanged;
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            int length = (int)numericPasswordLength.Value;
            string password = GeneratePassword(length);
            textBoxPassword.Text = password;
        }

        private string GeneratePassword(int length)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
            StringBuilder res = new StringBuilder();
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];

                while (length-- > 0)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    res.Append(validChars[(int)(num % (uint)validChars.Length)]);
                }
            }
            return res.ToString();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string account = textBoxAccount.Text;
            string password = textBoxPassword.Text;

            if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите имя учётной записи и сгенерируйте пароль!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string encryptedPassword = EncryptDecrypt(password);

            accounts[account] = encryptedPassword;
            listBoxAccounts.Items.Add(account);

            SaveDataToFile();


            textBoxAccount.Clear();
            textBoxPassword.Clear();
        }

        private void btnEncryptDecrypt_Click(object sender, EventArgs e)
        {
            string account = listBoxAccounts.SelectedItem?.ToString();

            if (account == null)
            {
                MessageBox.Show("Выберите учётную запись для шифрования/дешифрования!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string password = accounts[account];
            string encryptedPassword = EncryptDecrypt(password);
            accounts[account] = encryptedPassword;


            SaveDataToFile();

            MessageBox.Show($"Пароль для {account} был зашифрован/дешифрован: {encryptedPassword}", "Результат", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private string EncryptDecrypt(string input)
        {
            char key = 'K'; 
            StringBuilder output = new StringBuilder();

            foreach (char c in input)
            {
                output.Append((char)(c ^ key));
            }

            return output.ToString();
        }

        private void SaveDataToFile()
        {
            try
            {
                string json = JsonConvert.SerializeObject(accounts, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadDataFromFile()
        {
            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    accounts = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                    foreach (var account in accounts.Keys)
                    {
                        listBoxAccounts.Items.Add(account);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void listBoxAccounts_SelectedIndexChanged(object sender, EventArgs e)
        {
            string account = listBoxAccounts.SelectedItem?.ToString();

            if (account != null && accounts.ContainsKey(account))
            {
                string encryptedPassword = accounts[account];
                string decryptedPassword = EncryptDecrypt(encryptedPassword);

                textBoxPassword.Text = decryptedPassword;
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}

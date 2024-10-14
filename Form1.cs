using BUS4;
using DAL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using DAL4.entities;
using System.Data.Entity;
using System.Reflection;

namespace MoHinh3Lop
{
    public partial class Form1 : Form
    {
        private StudentBUS studentBLL;
        private QuanLySinhVienModel dbContext { get; set; }

        public Form1()
        {
            InitializeComponent();
            studentBLL = new StudentBUS();
            InitializeDataGridView();
            LoadComboBoxData();
            LoadStudentData();
           

        }
        private void LoadStudentData()
        {
            var studentList = studentBLL.GetAllStudents();
            BindGrid(studentList);
        }
        private void InitializeDataGridView()
        {
            dgvStudent.Columns.Add("MSSV", "Mã SV");
            dgvStudent.Columns.Add("HoTen", "Họ Tên");
            dgvStudent.Columns.Add("Khoa", "Khoa");
            dgvStudent.Columns.Add("DiemTB", "Điểm TB");
            dgvStudent.Columns.Add("ChuyenNganh", "Chuyên Ngành");
        }
        private void LoadComboBoxData()
        {
            var faculties = studentBLL.GetAllFaculties();
            cmbFaculty.DataSource = faculties;
            cmbFaculty.DisplayMember = "FacultyName";
            cmbFaculty.ValueMember = "FacultyID";


        }
        private void BindGrid(List<Student> listStudent)
        {
            dgvStudent.Rows.Clear();
            foreach (var item in listStudent)
            {
                int index = dgvStudent.Rows.Add();
                dgvStudent.Rows[index].Cells["MSSV"].Value = item.StudentID;
                dgvStudent.Rows[index].Cells["HoTen"].Value = item.FullName;
                dgvStudent.Rows[index].Cells["Khoa"].Value = item.Faculty != null ? item.Faculty.FacultyName : "";
                dgvStudent.Rows[index].Cells["DiemTB"].Value = item.AverageScore;
                dgvStudent.Rows[index].Cells["ChuyenNganh"].Value = item.Major != null ? item.Major.Name : "Chưa đăng ký";
                ShowAvatar(item.Avatar);
            }
        }
        private void ShowAvatar(string imageName)
        {
            if (string.IsNullOrEmpty(imageName))
            {
                picAvatar.Image = null;
            }
            else
            {
                string imagePath = Path.Combine("C:\\Users\\dell-pc\\source\\repos\\Images", imageName);
                if (File.Exists(imagePath))
                {
                    try
                    {
                        using (var fs = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                        {
                            picAvatar.Image = Image.FromStream(fs);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Không thể tải ảnh: {ex.Message}");
                    }
                    picAvatar.Refresh();
                }
                else
                {
                    picAvatar.Image = null;
                }
            }
        }

            private void dgvStudent_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void dgvStudent_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvStudent.Rows[e.RowIndex];
                txtMaSV.Text = row.Cells["MSSV"].Value?.ToString() ?? "";
                txtTenSV.Text = row.Cells["HoTen"].Value?.ToString() ?? "";
                cmbFaculty.Text = row.Cells["Khoa"].Value?.ToString() ?? "";
                txtDTB.Text = row.Cells["DiemTB"].Value?.ToString() ?? "";

                string avatarFileName = row.Cells["MSSV"].Value?.ToString() + ".jpg";
                ShowAvatar(avatarFileName);
            }
        }

        private void chkUnregisterMajor_CheckedChanged(object sender, EventArgs e)
        {
            var listStudents = chkUnregisterMajor.Checked ? studentBLL.GetAllStudentsWithNoMajor() : studentBLL.GetAllStudents();
            BindGrid(listStudents);
        }

        private void btnAddUpdate_Click(object sender, EventArgs e)
        {
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(txtMaSV.Text))
            {
                err.SetError(txtMaSV, "Vui lòng nhập mã sinh viên");
                isValid = false;
            }
            else
            {
                err.SetError(txtMaSV, string.Empty);
            }

            if (string.IsNullOrWhiteSpace(txtTenSV.Text))
            {
                err.SetError(txtTenSV, "Vui lòng nhập tên sinh viên");
                isValid = false;
            }
            else
            {
                err.SetError(txtTenSV, string.Empty);
            }

            if (cmbFaculty.SelectedValue == null)
            {
                err.SetError(cmbFaculty, "Vui lòng chọn khoa");
                isValid = false;
            }
            else
            {
                err.SetError(cmbFaculty, string.Empty);
            }

            if (string.IsNullOrWhiteSpace(txtDTB.Text))
            {
                err.SetError(txtDTB, "Vui lòng nhập điểm trung bình");
                isValid = false;
            }
            else
            {
                double averageScore;
                if (!double.TryParse(txtDTB.Text, out averageScore))
                {
                    err.SetError(txtDTB, "Điểm trung bình không hợp lệ");
                    isValid = false;
                }
                else
                {
                    err.SetError(txtDTB, string.Empty);
                }
            }

            if (isValid)
            {
                try
                {
                    string studentID = txtMaSV.Text;
                    string fullName = txtTenSV.Text;
                    double averageScore;
                    double.TryParse(txtDTB.Text, out averageScore);
                    int facultyID = int.Parse(cmbFaculty.SelectedValue.ToString());
                    string avatarFileName = SaveAvatar(studentID);

                    Student student = new Student
                    {
                        StudentID = studentID,
                        FullName = fullName,
                        AverageScore = averageScore,
                        FacultyID = facultyID,
                        Avatar = avatarFileName
                    };

                    var existingStudent = studentBLL.GetStudentById(studentID);
                    if (existingStudent == null)
                    {
                        studentBLL.AddOrUpdateStudent(student);
                        MessageBox.Show("Thêm sinh viên mới thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        studentBLL.AddOrUpdateStudent(student);
                        MessageBox.Show("Cập nhật thông tin sinh viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    LoadStudentData();
                    ShowAvatar(student.Avatar);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }


        private string SaveAvatar(string studentID)
        {
           if (picAvatar.Image != null && picAvatar.Tag != null)
            {
                string originalFilePath = picAvatar.Tag.ToString();
                string fileExtension = Path.GetExtension(originalFilePath); 
                string imageName = studentID + fileExtension;
                string imagesDirectory = "C:\\Users\\dell-pc\\source\\repos\\Images";
                string savePath = Path.Combine(imagesDirectory, imageName);
                try
                {
                    if (!Directory.Exists(imagesDirectory))
                    {
                        Directory.CreateDirectory(imagesDirectory);
                    }

                    File.Copy(originalFilePath, savePath, true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Không thể lưu ảnh: {ex.Message}");
                }
                return imageName;
            }
            return null;
        }


        private void btnChooseImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files (*.jpg; *.jpeg; *.png)|*.jpg; *.jpeg; *.png";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var originalImage = Image.FromFile(openFileDialog.FileName))
                        {
                            picAvatar.Image = new Bitmap(originalImage);
                        }

                        picAvatar.Tag = openFileDialog.FileName;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Không thể mở ảnh: {ex.Message}");
                    }
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                string studentID = txtMaSV.Text;
                if (MessageBox.Show("Bạn có chắc chắn muốn xóa sinh viên này?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    studentBLL.DeleteStudent(studentID);
                    string imagesDirectory = "C:\\Users\\dell-pc\\source\\repos\\Images";
                    string avatarPath = Path.Combine(imagesDirectory, studentID + ".jpg");
                    if (File.Exists(avatarPath))
                    {
                        File.Delete(avatarPath);
                    }
                    avatarPath = Path.Combine(imagesDirectory, studentID + ".png");
                    if (File.Exists(avatarPath))
                    {
                        File.Delete(avatarPath);
                    }
                    LoadStudentData();
                    MessageBox.Show("Xóa sinh viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void đăngKíChuyênNgànhToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmDangKiChuyenNganh dangKiChuyenNganh = new frmDangKiChuyenNganh(studentBLL);
            dangKiChuyenNganh.Owner = this;
            var result = dangKiChuyenNganh.ShowDialog();
            if (result == DialogResult.OK)
            {
                LoadStudentData();
            }
        }
            private void chứcNăngToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void cmbMajor_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
    
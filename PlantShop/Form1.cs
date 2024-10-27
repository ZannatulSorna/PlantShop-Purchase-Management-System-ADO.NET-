using PlantShop.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PlantShop
{
    public partial class Form1 : Form
    {
        string conStr = ConfigurationManager.ConnectionStrings["con"].ConnectionString;
        int intCustId = 0;
        string strPreviousImage = "";
        bool defaultImage = true;
        OpenFileDialog ofd = new OpenFileDialog();
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadCategoryCmb();
            LoaddgvCustomerList();
            Clear();

        }

        private void Clear()
        {
            txtCustCode.Text = "";
            txtCustName.Text = "";
            cmbCategory.SelectedIndex = 0;
            dtpDOB.Value = DateTime.Now;
            rbtnMale.Checked = true;
            chkActive.Checked = true;
            intCustId = 0;
            btnDelete.Enabled = false;
            btnSave.Text = "Save";
            pictureBoxCustomer.Image = Image.FromFile(Application.StartupPath + "\\images\\noimage.png");
            defaultImage = true;
            if (dgvPlant.DataSource == null)
            {
                dgvPlant.Rows.Clear();
            }
            else
            {
                dgvPlant.DataSource = (dgvPlant.DataSource as DataTable).Clone();
            }
        }

        private void LoaddgvCustomerList()
        {
            using (SqlConnection con = new SqlConnection(conStr))
            {
                con.Open();
                SqlDataAdapter sda = new SqlDataAdapter("ViewAllCustomers", con);
                sda.SelectCommand.CommandType = CommandType.StoredProcedure;
                DataTable dt = new DataTable();

                sda.Fill(dt);
                dt.Columns.Add("Image", Type.GetType("System.Byte[]"));
                foreach (DataRow dr in dt.Rows)
                {
                    dr["Image"] = File.ReadAllBytes(Application.StartupPath + "\\images\\" + dr["ImagePath"].ToString());
                }
                dgvCustomerList.RowTemplate.Height = 80;
                dgvCustomerList.DataSource = dt;

                ((DataGridViewImageColumn)dgvCustomerList.Columns[dgvCustomerList.Columns.Count - 1]).ImageLayout = DataGridViewImageCellLayout.Stretch;

                sda.Dispose();
            }
        }

        private void LoadCategoryCmb()
        {
            using (SqlConnection con = new SqlConnection(conStr))
            {
                con.Open();
                SqlDataAdapter sda = new SqlDataAdapter("SELECT * FROM Category", con);
                DataTable dt = new DataTable();
                sda.Fill(dt);
                DataRow topRow = dt.NewRow();
                topRow[0] = 0;
                topRow[1] = "--Select--";
                dt.Rows.InsertAt(topRow, 0);
                cmbCategory.ValueMember = "CategoryId";
                cmbCategory.DisplayMember = "CategoryTitle";
                cmbCategory.DataSource = dt;
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            Clear();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            ofd.Filter = "Images(.jpg,.png,.png)|*.png;*.jpg; *.png";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                pictureBoxCustomer.Image = new Bitmap(ofd.FileName);
                if (intCustId == 0)
                {
                    defaultImage = false;
                    strPreviousImage = "";
                }

            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            pictureBoxCustomer.Image = new Bitmap(Application.StartupPath + "\\images\\noimage.png");
            defaultImage = true;
            strPreviousImage = "";
        }
        bool ValidateMasterDetailForm()
        {
            bool isValid = true;
            if (txtCustName.Text.Trim() == "")
            {
                MessageBox.Show("Customer name is required");
                isValid = false;
            }
            return isValid;
        }
        string SaveImage(string imgPath)
        {
            string fileName = Path.GetFileNameWithoutExtension(imgPath);
            string ext = Path.GetExtension(imgPath);
            fileName = fileName.Length <= 15 ? fileName : fileName.Substring(0, 15);
            fileName = fileName + DateTime.Now.ToString("yymmssfff") + ext;
            pictureBoxCustomer.Image.Save(Application.StartupPath + "\\images\\" + fileName);
            return fileName;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (ValidateMasterDetailForm())
            {
                int custId = 0;
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("CustomerAddOrEdit", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CustomerId", intCustId);
                    cmd.Parameters.AddWithValue("@CustomerCode", txtCustCode.Text.Trim());
                    cmd.Parameters.AddWithValue("@CustomerName", txtCustName.Text.Trim());
                    cmd.Parameters.AddWithValue("@CategoryId", Convert.ToInt16(cmbCategory.SelectedValue));
                    cmd.Parameters.AddWithValue("@DateOfBuying", dtpDOB.Value);
                    cmd.Parameters.AddWithValue("@IsActive", chkActive.Checked ? "True" : "False");
                    cmd.Parameters.AddWithValue("@Gender", rbtnMale.Checked ? "Male" : "Female");
                    if (defaultImage)
                    {
                        cmd.Parameters.AddWithValue("@ImagePath", DBNull.Value);
                    }

                    else if (intCustId > 0 && strPreviousImage != "")
                    {
                        cmd.Parameters.AddWithValue("@ImagePath", strPreviousImage);
                        if (ofd.FileName != strPreviousImage)
                        {
                            var filename = Application.StartupPath + "\\images\\" + strPreviousImage;
                            if (pictureBoxCustomer.Image != null)
                            {
                                pictureBoxCustomer.Image.Dispose();
                                pictureBoxCustomer.Image = null;
                                System.IO.File.Delete(filename);
                            }
                        }

                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@ImagePath", SaveImage(ofd.FileName));
                    }
                    custId = Convert.ToInt16(cmd.ExecuteScalar());
                }
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    con.Open();
                    foreach (DataGridViewRow item in dgvPlant.Rows)
                    {
                        if (item.IsNewRow) break;
                        else
                        {
                            SqlCommand cmd = new SqlCommand("PlantAddAndEdit", con);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@PlantId", Convert.ToInt32(item.Cells["dgvPlantId"].Value == DBNull.Value ? "0" : item.Cells["dgvPlantId"].Value));
                            cmd.Parameters.AddWithValue("@CustomerId", custId);
                            cmd.Parameters.AddWithValue("@PlantName", item.Cells["dgvPlantName"].Value);
                            cmd.Parameters.AddWithValue("@Price", item.Cells["dgvPricePayed"].Value);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                LoaddgvCustomerList();
                Clear();
                MessageBox.Show("Submitted Successfully");

            }
        }

        private void dgvCustomerList_DoubleClick(object sender, EventArgs e)
        {
            if (dgvCustomerList.CurrentRow.Index != -1)
            {
                DataGridViewRow dgvRow = dgvCustomerList.CurrentRow;
                intCustId = Convert.ToInt32(dgvRow.Cells[0].Value);
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    con.Open();
                    SqlDataAdapter sda = new SqlDataAdapter("ViewCustomerByCustomerId", con);
                    sda.SelectCommand.CommandType = CommandType.StoredProcedure;
                    sda.SelectCommand.Parameters.AddWithValue("@CustomerId", intCustId);
                    DataSet ds = new DataSet();
                    sda.Fill(ds);
                    //--Master---
                    DataRow dr = ds.Tables[0].Rows[0];
                    txtCustCode.Text = dr["CustomerCode"].ToString();
                    txtCustName.Text = dr["CustomerName"].ToString();
                    cmbCategory.SelectedValue = Convert.ToInt32(dr["CategoryId"].ToString());
                    dtpDOB.Value = Convert.ToDateTime(dr["DateOfBuying"].ToString());
                    if (Convert.ToBoolean(dr["IsActive"].ToString()))
                    {
                        chkActive.Checked = true;
                    }
                    else
                    {
                        chkActive.Checked = false;
                    }
                    if ((dr["Gender"].ToString().Trim()) == "Male")
                    {
                        rbtnMale.Checked = true;
                    }
                    else
                    {
                        rbtnMale.Checked = false;
                    }
                    if ((dr["Gender"].ToString().Trim()) == "Female")
                    {
                        rbtnFemale.Checked = true;
                    }
                    else
                    {
                        rbtnFemale.Checked = false;
                    }
                    if (dr["ImagePath"] == DBNull.Value)
                    {
                        pictureBoxCustomer.Image = new Bitmap(Application.StartupPath + "\\images\\noimage.png");
                    }
                    else
                    {
                        string image = dr["ImagePath"].ToString();
                        pictureBoxCustomer.Image = new Bitmap(Application.StartupPath + "\\images\\" + dr["ImagePath"].ToString());
                        strPreviousImage = dr["ImagePath"].ToString();
                        defaultImage = false;
                    }
                    //--Details---
                    dgvPlant.AutoGenerateColumns = false;
                    dgvPlant.DataSource = ds.Tables[1];
                    btnDelete.Enabled = true;
                    btnSave.Text = "Update";
                    tabControl1.SelectedIndex = 0;
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure to delete this record?", "Master Details", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                string image = "";
                using (SqlConnection con = new SqlConnection(conStr))
                {
                    con.Open();
                    SqlDataAdapter sda = new SqlDataAdapter("ViewCustomerByCustomerId", con);
                    sda.SelectCommand.CommandType = CommandType.StoredProcedure;
                    sda.SelectCommand.Parameters.AddWithValue("@CustomerId", intCustId);
                    DataSet ds = new DataSet();
                    sda.Fill(ds);
                    DataRow dr = ds.Tables[0].Rows[0];
                    if (dr["ImagePath"] != DBNull.Value)
                    {
                        image = dr["ImagePath"].ToString();
                        var filename = Application.StartupPath + "\\images\\" + image;
                        if (pictureBoxCustomer.Image != null)
                        {
                            pictureBoxCustomer.Image.Dispose();
                            pictureBoxCustomer.Image = null;
                            System.IO.File.Delete(filename);
                        }

                    }
                    SqlCommand cmd = new SqlCommand("CustomerPlantDelete", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@CustomerId", intCustId);
                    sda.Dispose();
                    cmd.ExecuteNonQuery();
                    LoaddgvCustomerList();
                    Clear();
                    MessageBox.Show("Deleted Successfully");
                }
                // File.Delete(filePath);
            }
        }

        private void dgvPlant_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            DataGridViewRow dgvRow = dgvPlant.CurrentRow;
            if (dgvRow.Cells["dgvPlantId"].Value != DBNull.Value)
            {
                if (MessageBox.Show("Are you sure to delete this record?", "Master Details", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    using (SqlConnection con = new SqlConnection(conStr))
                    {
                        con.Open();
                        SqlCommand cmd = new SqlCommand("PlantDelete", con);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@PlantId", dgvRow.Cells["dgvPlantId"].Value);
                        cmd.ExecuteNonQuery();
                    }

                }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            using (SqlConnection con = new SqlConnection(conStr))
            {
                con.Open();
                SqlDataAdapter sda = new SqlDataAdapter("ViewAllCustomers", con);
                sda.SelectCommand.CommandType = CommandType.StoredProcedure;
                DataTable dt = new DataTable();
                sda.Fill(dt);
                List<CustomerViewModel> list = new List<CustomerViewModel>();
                CustomerViewModel customerVm;
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        customerVm = new CustomerViewModel();
                        customerVm.CustomerId = Convert.ToInt32(dt.Rows[i]["CustomerId"]);
                        customerVm.CustomerCode = dt.Rows[i]["CustomerCode"].ToString();
                        customerVm.CustomerName = dt.Rows[i]["CustomerName"].ToString();
                        customerVm.DateOfBuying = Convert.ToDateTime(dt.Rows[i]["DateOfBuying"].ToString());
                        customerVm.Gender = dt.Rows[i]["Gender"].ToString();
                        customerVm.IsActive = Convert.ToBoolean(dt.Rows[i]["IsActive"].ToString());
                        customerVm.TotalPrice = Convert.ToInt32(dt.Rows[i]["TotalPrice"]);
                        customerVm.CategoryTitle = dt.Rows[i]["CategoryTitle"].ToString();
                        customerVm.ImagePath = Application.StartupPath + "\\images\\" + dt.Rows[i]["ImagePath"].ToString();
                        list.Add(customerVm);

                    }
                    using (CustomerReport report = new CustomerReport(list))
                    {
                        report.ShowDialog();
                    }
                }


            }
        }
    }
}

            
        
    


using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace TKSE.FishID.Web
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lblMessage.Visible = false;
                lblMessage2.Visible = false;
            }
        }

        protected void BtnUpload_Click(object sender, EventArgs e)
        {
            HttpPostedFile postedFile = FileUpload1.PostedFile;
            string filename = Path.GetFileName(postedFile.FileName);
            string fileExtension = Path.GetExtension(filename);
            int fileSize = postedFile.ContentLength;

            if (fileExtension.ToLower() == ".jpg" || fileExtension.ToLower() == ".gif"
                || fileExtension.ToLower() == ".png" || fileExtension.ToLower() == ".bmp")
            {
                Stream stream = postedFile.InputStream;
                BinaryReader binaryReader = new BinaryReader(stream);
                byte[] bytes = binaryReader.ReadBytes((int)stream.Length);

                string cs = ConfigurationManager.ConnectionStrings["DBCS1"].ConnectionString;

                using (SqlConnection con = new SqlConnection(cs))
                {
                    SqlCommand cmd = new SqlCommand("spUploadImage", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlParameter paramName = new SqlParameter()
                    {
                        ParameterName = "@Name",
                        Value = filename
                    };
                    cmd.Parameters.Add(paramName);

                    SqlParameter paramSize = new SqlParameter()
                    {
                        ParameterName = "@Size",
                        Value = fileSize
                    };
                    cmd.Parameters.Add(paramSize);

                    SqlParameter paramImageData = new SqlParameter()
                    {
                        ParameterName = "@ImageData",
                        Value = bytes
                    };
                    cmd.Parameters.Add(paramImageData);

                    SqlParameter paramNewId = new SqlParameter()
                    {
                        ParameterName = "@NewId",
                        Value = -1,
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(paramNewId);

                    SqlParameter paramDateModified = new SqlParameter()
                    {
                        ParameterName = "@DateModified",
                        Value = DateTime.Now // Or you can use DateTime.UtcNow for UTC time
                    };
                    cmd.Parameters.Add(paramDateModified);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();

                    lblMessage.Visible = true;
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                    lblMessage.Text = "Upload Successful";

                }
            }
            else
            {
                lblMessage.Visible = true;
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Text = "Only images (.jpg, .png, .gif, and .bmp) can be uploaded";

            }
        }

        protected void BtnCount_Click(object sender, EventArgs e)
        {
            string consoleAppPath = @"C:\inetpub\wwwroot\FishIDCPU\TKSE_FishClassification_MLModel_ConsoleApp_CPU.exe";
                System.Diagnostics.Process Proc = new System.Diagnostics.Process();
                //Proc.StartInfo.FileName = @"C:\inetpub\wwwroot\FishIDCPU\TKSE_FishClassification_MLModel_ConsoleApp_CPU.exe";
                //Proc.Start();


            //if (Proc.Start() == true)
            //{
            //    Response.Write("App Executed");
            //}

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = consoleAppPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,   //==================== Should be false
                    CreateNoWindow = true
                };

                using (Process process = new Process())
                {
                    process.StartInfo = startInfo;
                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        string[] lines = output.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                        lblMessage2.Visible = true;
                        lblMessage2.ForeColor = System.Drawing.Color.Green;
                        lblMessage2.Text = string.Join("<br/>", lines);
                    }

                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        lblMessage.Visible = true;  //=============== shold be false to hide  the errors
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                        lblMessage.Text = error;
                    }

                    // Call the ImageView method to display the latest image
                    ImageView();
                }
            }
            catch (Exception ex)
            {
                lblMessage.Visible = true;
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Text = $"Error executing the console application: {ex.Message}";
            }
        }

        // Modified the ImageView method to update the Image control
        private void ImageView()
        {
            string cs = ConfigurationManager.ConnectionStrings["DBCS1"].ConnectionString;

            using (SqlConnection con = new SqlConnection(cs))
            {
                SqlCommand cmd = new SqlCommand("spGetLatestImage", con);
                cmd.CommandType = CommandType.StoredProcedure;

                con.Open();
                byte[] bytes = (byte[])cmd.ExecuteScalar();
                string strBase64 = Convert.ToBase64String(bytes);
                Image1.ImageUrl = "data:image/png;base64," + strBase64;
            }
        }



    }
}

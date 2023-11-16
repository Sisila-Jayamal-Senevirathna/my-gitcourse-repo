using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FishCountImageUpload
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                lblMessage.Visible = false;
                hyperlink.Visible = false;
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
                Byte[] bytes = binaryReader.ReadBytes((int)stream.Length);


                string cs = ConfigurationManager.ConnectionStrings["DBCS1"].ConnectionString;
                using (SqlConnection con = new SqlConnection(cs))
                {
                    SqlCommand cmd = new SqlCommand("spUploadImage", con);
                    cmd.CommandType = CommandType.StoredProcedure;

                    SqlParameter paramName = new SqlParameter()
                    {
                        ParameterName = @"Name",
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
                    hyperlink.Visible = true;
                    hyperlink.NavigateUrl = "~/WebForm2.aspx?Id=" +
                        cmd.Parameters["@NewId"].Value.ToString();
                }
            }
            else
            {
                lblMessage.Visible = true;
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Text = "Only images (.jpg, .png, .gif and .bmp) can be uploaded";
                hyperlink.Visible = false;
            }
        }

        protected void BtnCount_Click(object sender, EventArgs e)
        {
            // Provide the path to the MLModel1_ConsoleApp3.exe file in your project's output directory.
            string consoleAppPath = @"C:\inetpub\wwwroot\GoldFishCountMLModelConsoleApp\GoldFishCounterMLModel_ConsoleApp.exe";
            //string consoleAppPath = @"C:\inetpub\wwwroot\web\project\bin\MLModel1_ConsoleApp3.exe";


            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = consoleAppPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
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
                        // Output from the console application (if any).
                        // You can display it in a label or any other control on your web page.
                        lblMessage2.Visible = true;
                        lblMessage2.ForeColor = System.Drawing.Color.Green;
                        //lblMessage2.Text = output;
                        lblMessage2.Text = string.Join("<br/>", lines);
                    }

                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        // Error from the console application (if any).
                        // You can display it in a label or any other control on your web page.
                        lblMessage.Visible = true;
                        lblMessage.ForeColor = System.Drawing.Color.Red;
                        lblMessage.Text = error;
                    }

                    string cs = ConfigurationManager.ConnectionStrings["DBCS1"].ConnectionString;
                    using (SqlConnection con = new SqlConnection(cs))
                    {
                        SqlCommand cmd = new SqlCommand("spGetLatestImage", con); // Modified stored procedure name
                        cmd.CommandType = CommandType.StoredProcedure;

                        con.Open();
                        byte[] bytes = (byte[])cmd.ExecuteScalar();
                        string strBase64 = Convert.ToBase64String(bytes);
                        Image1.ImageUrl = "data:image/png;base64," + strBase64; // Make sure to use the appropriate image format here
                    }

                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during the process execution.
                lblMessage.Visible = true;
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Text = $"Error executing the console application: {ex.Message}";
            }
        }
       
    }
}

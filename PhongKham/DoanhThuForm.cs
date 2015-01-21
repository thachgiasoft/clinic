﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Clinic.Database;
using Clinic.Helpers;
using Clinic.Models;
using PhongKham;
using Clinic.Models.ItemMedicine;

namespace Clinic
{



    public partial class DoanhThuForm : Form
    {

        private List<ItemDoanhThu> listItem;
        private string tongDoanhThu="0";
        private int tongLuotKham=0;
        List<IMedicine> currentMedicines;
        List<IMedicine> currentServices;
        List<string> AllLoaiKham;


        public DoanhThuForm()
        {
            InitializeComponent();

            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.ColumnServices.DefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            currentMedicines = Helper.GetAllMedicineFromDb();
            currentServices = Helper.GetAllServiceFromDb();
            AllLoaiKham = Helper.GetAllLoaiKham(DatabaseFactory.Instance);

        }

        public void FillToGrid(List<ItemDoanhThu> listItem)
        {
            List<DoanhThuBacSi> listBacSi = new List<DoanhThuBacSi>();
            Dictionary<string, int> listService = new Dictionary<string,int>();

            List<string> listID = new List<string>();

            for (int i = 0; i < listItem.Count; i++)
            {

                int index = dataGridView1.Rows.Add();
                DataGridViewRow row = dataGridView1.Rows[index];
                row.Cells["STT"].Value = i + 1;
                row.Cells[1].Value = listItem[i].Date;


                   
                row.Cells[3].Value = listItem[i].Money;
                row.Cells["ColumnIdPatient"].Value = listItem[i].IdPatient;
                if (!listID.Contains(listItem[i].IdPatient))
                {
                    listID.Add(listItem[i].IdPatient);

                }

                row.Cells["ColumnNamePatient"].Value = listItem[i].NamePatient;

                string nameDoctor= listItem[i].NameOfDoctor;
                row.Cells[2].Value = nameDoctor;
                row.Cells["ColumnServices"].Value = BuildStringServicesAndAdmin(listItem[i].Services, ref listService);
                row.Cells["ColumnLoaiKham"].Value = listItem[i].LoaiKham;

                DoanhThuBacSi bsTemp = listBacSi.Where(x => x.NameBacSi == nameDoctor).FirstOrDefault();
                if (bsTemp == null)
                {
                    bsTemp = new DoanhThuBacSi();
                    bsTemp.NameBacSi = nameDoctor;
                    bsTemp.SoLuotKham = 1;
                    bsTemp.SoTien = listItem[i].Money;
                    listBacSi.Add(bsTemp);
                }
                else
                {
                    bsTemp.SoLuotKham++;
                    bsTemp.SoTien += listItem[i].Money;
                }     

            }


            dataGridView3.Rows.Clear();

             
            foreach (string keyService in listService.Keys)
            {

                    int index = dataGridView3.Rows.Add();
                    DataGridViewRow row = dataGridView3.Rows[index];
                    row.Cells["ColumnServiceName"].Value = keyService;
                    row.Cells["ColumnServiceAdmin"].Value = currentServices.Where(x => x.Name == keyService).FirstOrDefault().Admin;
                    row.Cells["ColumnServiceCount"].Value = listService[keyService].ToString();
                    row.Cells["ColumnServiceTotal"].Value = (listService[keyService] * currentServices.Where(x => x.Name == keyService).FirstOrDefault().CostOut).ToString();
                
            }


            dataGridView2.Rows.Clear();
            //each doctor
            for (int i = 0; i < listBacSi.Count; i++)
            {
                int index = dataGridView2.Rows.Add();
                DataGridViewRow row = dataGridView2.Rows[index];
                row.Cells["G2NameDoctor"].Value = listBacSi[i].NameBacSi;
                row.Cells["G2SoLuotKham"].Value = listBacSi[i].SoLuotKham.ToString();
                row.Cells["G2TongCong"].Value = listBacSi[i].SoTien.ToString("C0");
            }


            dataGridView4.Rows.Clear();
            //each LoaiKham
            for (int i = 0; i < AllLoaiKham.Count; i++)
            {
                int index = dataGridView4.Rows.Add();
                DataGridViewRow row = dataGridView4.Rows[index];
                row.Cells[0].Value = AllLoaiKham[i];
                row.Cells[1].Value = listItem.Where(x => x.LoaiKham == AllLoaiKham[i]).Count();

            }

            this.PatientNumber.Text = listID.Count.ToString();

        }

        private string BuildStringServicesAndAdmin(string servicesWithoutAdmin, ref Dictionary<string,int> listService)
        {
            string result = "";
            string[] serviceArray = servicesWithoutAdmin.Split(new string[] {ClinicConstant.StringBetweenServicesInDoanhThu}, StringSplitOptions.None);

            for (int i = 0; i < serviceArray.Count(); i++)
            {
                IMedicine service = currentServices.Where(x => x.Name == serviceArray[i]).FirstOrDefault();
                result += (serviceArray[i] + ClinicConstant.StringBetweenServiceAndAdmin + (service==null?"": service.Admin));
                if (i != serviceArray.Count() - 1)
                {
                    result += "\n";
                }
                if ((!String.IsNullOrEmpty(serviceArray[i])) && serviceArray[i][0] == '@')
                {
                    if (listService.ContainsKey(serviceArray[i]))
                    {
                        listService[serviceArray[i]]++;
                    }
                    else
                    {
                        listService.Add(serviceArray[i], 1);
                    }
                }
            }

            return result;
        }

        private void button1_Click(object sender, EventArgs e) // ngay
        {
            dataGridView1.Rows.Clear();
             listItem  = Helpers.Helper.DoanhThuTheoNgay(DatabaseFactory.Instance,dateTimePicker1.Value);
             FillToGrid(listItem);
             CalcuTotal();
        }

        private void button2_Click(object sender, EventArgs e) // thang
        {
            dataGridView1.Rows.Clear();
            listItem = Helpers.Helper.DoanhThuTheoThang(DatabaseFactory.Instance, dateTimePicker1.Value);
            FillToGrid(listItem);
            CalcuTotal();
        }


        private void CalcuTotal()
        {
            int total =0;
            for (int i = 0; i < dataGridView1.Rows.Count-1; i++)
            {
                DataGridViewRow row = dataGridView1.Rows[i];
                total += int.Parse(row.Cells[3].Value.ToString());
            }
            labelTotal.Text = total.ToString("C0");
            this.tongDoanhThu = labelTotal.Text;
            this.tongLuotKham = int.Parse(this.PatientNumber.Text);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string namePDF = "DoanhThu";
            Helper.CreateAPdfThongKeDoanhThu(this.dataGridView1, namePDF,tongLuotKham,tongDoanhThu);
            this.PDFShowDoanhThu.LoadFile("DoanhThu.pdf");
        }
    }
}

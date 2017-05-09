using AuthorizeNet.Api.Contracts.V1;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Mail;
using System.Text;
using System.Net.Mime;

namespace Campco.Common
{
    public partial class Thankyou : System.Web.UI.Page
    {     
        dbUtility dbUtl = new dbUtility();
        public List<State> stateList;
        public List<Product> Products = null;
       // public List<Recommand> Products = null;
        public double totalAmount = 0.00;
        public string cusID = "";     
        public int drop = 0;
        public string CustomerName = "";//SessionVariable.CustomerID;
        public string orderNumber = "";//SessionVariable.orderID.ToString();
        public decimal total = 0;//SessionVariable.Amount;
        public double shippingCharge = 0;//SessionVariable.ShippingCharge;
        public string Time = "";//DateTime.UtcNow.ToString();
        public double subtotal = 0;
        public int custype = 0;
        public customerAddressType Address = null;
        public List<Product> RelatedProd = null;// nisha patel -12-04-2017 (Related product.)
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                HttpContext.Current.Session["sort"] = null;
                if (!IsPostBack)
                {
                    CustomerName = SessionVariable.CustomerName;
                    orderNumber = SessionVariable.orderID.ToString();
                    total = SessionVariable.Amount;
                    shippingCharge = SessionVariable.ShippingCharge;
                    Time = DateTime.UtcNow.ToString();
                    var cart = SessionVariable.AddToCart;
                    drop = Convert.ToInt32(HttpContext.Current.Session["drop"]);
                    custype = SessionVariable.customerType;
                    Address = SessionVariable.ShipAddress;
                   LinkedResource Img;
                    string pth1 ="";
                    if (cart != null)
                    {
                        dbUtility dbUtl = new dbUtility();
                        Products = dbUtl.Cart(cart);
 

                        foreach (var item in Products)
                        {
                            totalAmount += (SessionVariable.customerType == (int)custtype.Retailer ? item.RETAIL_PRS : SessionVariable.IsSpecial > 0 ? item.FinalPrice : item.WHOLE_PRS) * item.QTYinCart;
                            item.RETAIL_PRS = SessionVariable.customerType == (int)custtype.Retailer ? item.RETAIL_PRS : SessionVariable.IsSpecial > 0 ? item.FinalPrice : item.WHOLE_PRS;
                        }

                        foreach (var item in Products)
                        {
                            var path = Server.MapPath(@"~/Pic/" + item.SMALLPIC);
                            var pth = File.Exists(path) ? "../Pic/" + item.SMALLPIC : "../common/assets/images/placeholder.png";
                            item.SMALLPIC = pth;
                        }

                        subtotal = totalAmount;
                        SessionVariable.QTYCHARGE = totalAmount;
                        SessionVariable.cart_Count = SessionVariable.AddToCart == null ? 0 : Convert.ToInt32(SessionVariable.AddToCart.Compute("SUM(QTY)", ""));
                        if (SessionVariable.customerType == (int)custtype.Retailer || SessionVariable.CustomerID.StartsWith("G#"))
                        {
                            SessionVariable.ShippingCharge = Convert.ToDouble(dbUtl.ShippingCharge(SessionVariable.cart_Count, Convert.ToDecimal(totalAmount)));
                        }
                        else
                        {
                            SessionVariable.ShippingCharge = Convert.ToDouble(HttpContext.Current.Session["drop"] != null ? HttpContext.Current.Session["drop"] : 0);
                        }
                    }
                    CustomerName = SessionVariable.CustomerName;
                    orderNumber = SessionVariable.orderID.ToString();
                    total = Convert.ToDecimal(totalAmount + SessionVariable.ShippingCharge); //Convert.ToDecimal((SessionVariable.Amount).ToString("0.00"));
                    shippingCharge = Convert.ToDouble((SessionVariable.ShippingCharge).ToString("0.00"));
                    //if (HttpContext.Current.Session["drop"].ToString() == "3")
                    //totalAmount += 3;
                    //total = total+Convert.ToDecimal(shippingCharge);
                    SessionVariable.Amount = (decimal)totalAmount + Convert.ToDecimal(SessionVariable.ShippingCharge);
                    //Harikrishna Parmar/13-10-2016/End

                    string str = "";
                    str += "<table border='1'>";
                    str += "<thead>";
                    str += "<tr>";
                    str += "<td width='40%'><h5>Description</h5></td><td width='20%'><h5>Qty</h5></td><td width='20%'><h5>Price</h5></td><td width='20%'><h5>Item Total</h5></td>";
                    str += "</tr>";
                    str += "</thead>";
                    str += "<tbody>";
                    foreach (var item in Products)
                    {
                        str += "<tr>";
                        str += "<td>" + item.PROD_CD + "<p>" + item.DESCRIP + "</p></td>";
                        str += "<td><p>" + item.QTYinCart + "</p></td>";
                        str += "<td><p>$" + item.RETAIL_PRS + "</p></td>";
                        str += "<td><p>$" + item.RETAIL_PRS * item.QTYinCart + "</p></td>";
                        str += "</tr>";
                    }
                    str += "<tr><td class='thick - line'></td><td class='thick - line'></td><td class='thick - line text - center'><strong>Sub Total:</strong></td><td class='thick - line text - right'>$" + subtotal + "</td></tr>";
                    if (custype == 3)
                    {
                        if (drop > 0)
                        {
                            str += "<tr><td class='no - line'></td><td class='no - line'></td><td class='no - line text - center'><strong>Drop Ship Fee:</strong></td><td class='no - line text - right'>$3.00</td></tr>";
                        }
                    }
                    else
                    {
                        if (shippingCharge > 0)
                        {
                            str += "<tr><td></td><td></td><td><strong>Shipping Charge :</strong></td><td >$" + Convert.ToDouble(shippingCharge).ToString("0.00") + "</td></tr>";
                        }
                    }
                    str += "<tr><td ></td><td ></td><td ><strong>Grand Total:</strong></td><td >$" + total + "</td></tr>";
                    str += "</tbody>";
                    str += "</table>";

                    string str1 = "";
                    str1 += "<table border='1'>";
                    str1 += "<thead>";
                    str1 += "<tr>";
                    foreach (var item1 in Products)
                    {
                        str1 += "<td width='40%'>" + item1.PROD_CD+ "</td>";
                    }
                    str1 += "</tr>";
                    str1 += "</thead>";
                    str1 += "<tbody>";

                    foreach (var item in Products)
                    {
                        RelatedProd = dbUtl.RelatedProduct(item.PROD_CD);
                        foreach (var relpro in RelatedProd)
                        {
                            var path = Server.MapPath("~/Pic/" + relpro.SMALLPIC);
                            pth1 = File.Exists(path) ? "E:/nisha/ordex/hR/Campco/Campco/Pic/" + relpro.SMALLPIC : "../common/assets/images/placeholder.png";
                            Img = new LinkedResource(path, MediaTypeNames.Image.Jpeg);                    
                            Img.ContentId = "MyImage";
                            str1 += "<tr>";
                            str1 += "<td>" + " " + "<img src='"+pth1+"' id='img' alt='no image found' width='400px' height='400px'/> </img></td>";      
                            str1 += "</tr>";

                        }
                    }



                    str1 += "</tbody>";
                    str1 += "</table>";

                    

                        var body = PopulateBody(str,str1);
                    try
                    {
                        dbUtl.SendEmaill(ConfigurationManager.AppSettings["CompanyEmailAddress"], ConfigurationManager.AppSettings["PasswordMail"], SessionVariable.UName, "Order Confirmation", body, ConfigurationManager.AppSettings["CompanyEmailAddress"],0, pth1,str1);
                    }
                    catch(Exception ex)
                    {

                    }
                    SessionVariable.AddToCart = null;
                    SessionVariable.cart_Count = 0;
                    SessionVariable.TempOrderID = 0;
                    SessionVariable.orderID = 0;
                    SessionVariable.InterNationalShipping_Charge = -1;
                    SessionVariable.ShippingCharge = 0.00;
                    HttpContext.Current.Session["drop"] = null;
                    if (SessionVariable.CustomerID == "GUEST")
                    {
                        SessionVariable.CustomerID = null;
                    }

                }
            }
            catch(Exception ex)
            {
                dbUtl.logerrors(ex);
            }
        }
        private string PopulateBody(string tab, string tab2)
        {
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(Server.MapPath("~/OrderSummeryEmail.html")))
            {
                body = reader.ReadToEnd();
            }          
            body = body.Replace("{username}", SessionVariable.CustomerName);
            body = body.Replace("{ordernumber}", SessionVariable.orderID.ToString());
            body = body.Replace("{datetime}", DateTime.Now.ToString());
            body = body.Replace("{table}", tab);
            body = body.Replace("{table2}", tab2);
            body = body.Replace("{ShipName}", Address.firstName +" "+Address.lastName);
            body = body.Replace("{Street}", Address.address  );
            body = body.Replace("{State}", Address.city+","+ Address.state + "- "+ Address.zip );
            body = body.Replace("{country}", Address.country);
            body = body.Replace("{Email}", Address.email);
            body = body.Replace("{PhoneNumber}", Address.phoneNumber);
            return body;
        }
    }
}

  
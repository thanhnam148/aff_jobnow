using Aff.WebApplication.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Aff.WebApplication.Helpers
{
    public static class HtmlHelpers
    {
        public static HtmlString AjaxPagingHelper(this HtmlHelper htmlHelper, string actionAjax, int countRecord, int pageSize, int currentPage, string sortColumn = null, string sortDirection = null)
        {

            currentPage = (currentPage <= 0) ? 1 : currentPage;
            pageSize = (countRecord <= 0) ? SystemConfiguration.PageSizeDefault : pageSize;

            var str = new StringBuilder();
            var countTotal = (countRecord * 1.0) / pageSize;
            if (countTotal != 0)
            {
                var totalPages = Convert.ToInt32(countTotal > 1 ? Math.Ceiling(countTotal) : 1);

                if (totalPages > 0)
                {
                    str.Append("<div class=\"d-flex justify-content-between\">");
                    var strShownumber = " từ " + ((currentPage - 1) * pageSize + 1) + " đến ";
                    if (countRecord > currentPage * pageSize)
                    {
                        strShownumber += (currentPage * pageSize).ToString();
                    }
                    else
                    {
                        strShownumber += countRecord.ToString();
                    }
                    str.Append("<div class=\"paging_show_number\" id=\"paging_show_number\" role=\"status\" aria-live=\"polite\">Hiển thị " + strShownumber + " trong " + countRecord + " bản ghi</div>");
                    str.Append("<ul class=\"pagination uk-pagination pagination-sm custom_pagination\">");
                    int pageMin, pageMax;
                    int prev, next;
                    int first = 1;
                    int last = totalPages;

                    //low
                    if (currentPage <= 3)
                        pageMin = 1;
                    else
                        pageMin = (currentPage - 3);

                    //this is previous
                    if (currentPage == 1)
                    {
                        prev = 0; // no need to show prev link
                        first = 0; //no need to show first link
                        str.Append("<li class='disabled'>");
                        //str.Append("<a href='javascript:void(0)'><i class='icon-arrow-first'></i></a>");
                        str.Append("<a href='javascript:void(0)'>Trang đầu</a>");
                        str.Append("</li>");
                        str.Append("<li class='disabled'>");
                        str.Append("<a class='paginate_button previous disabled btn-bg1'><</a>");
                    }
                    else
                    {
                        prev = currentPage - 1;
                        str.Append("<li class='cursor-pointer'>");
                        str.Append("<a class = 'first paginate_button'" +
                                   " onclick=\"" + actionAjax + "(" + first + ",'" + sortColumn + "','" + sortDirection + "', 'search')" + "\"> Trang đầu</i></a>");
                        str.Append("</li>");
                        str.Append("<li class='cursor-pointer'>");
                        str.Append("<a class = 'previous paginate_button'" +
                                   " onclick=\"" + actionAjax + "(" + prev + ",'" + sortColumn + "','" + sortDirection + "', 'search')" + "\"><</a>");
                        str.Append("</li>");
                    }

                    if ((totalPages - (pageMin + 6)) >= 0)
                    {
                        pageMax = pageMin + 6;
                    }
                    else
                    {
                        pageMax = totalPages;
                    }

                    for (var i = pageMin; i <= pageMax; i++)
                    {
                        string objAttrib;
                        string active;
                        if (i == currentPage)
                        {
                            active = "active uk-active";
                            objAttrib = "class = 'paginate_active btn-bg1'";
                            str.Append("<li class='cursor-pointer " + active + "'>");
                            str.Append("<span " + objAttrib + "\"> " + i + "</span>");
                        }
                        else
                        {
                            active = "paginate_button";
                            objAttrib = "class='paginate_button'";
                            str.Append("<li class='cursor-pointer " + active + "'>");
                            str.Append("<a " + objAttrib + " onclick=\"" + actionAjax + "(" + i + ",'" + sortColumn + "','" + sortDirection + "', 'search')" + "\"> " + i + "</a>");
                        }

                        str.Append("</li>");
                    }

                    //this is next
                    if (currentPage == totalPages)
                    {
                        next = 0; //no need to show the next link
                        last = 0; //no need to show the last link
                        str.Append("<li class='disabled'>");
                        str.Append("<a href='javascript:void(0)' class='paginate_button next btn-bg1'>></a>");
                        str.Append("</li>");
                        str.Append("<li class='disabled'>");
                        str.Append("<a href='javascript:void(0)' class='paginate_button'>Trang cuối</a></li>");
                    }
                    else
                    {
                        next = currentPage + 1;
                        str.Append("<li class='cursor-pointer'>");
                        str.Append("<a class = 'paginate_button next' " +
                                   " onclick=\"" + actionAjax + "(" + next + ",'" + sortColumn + "','" + sortDirection + "', 'search')" + "\">></a>");
                        str.Append("</li>");
                        str.Append("</li>");
                        str.Append("<li class='cursor-pointer'>");
                        str.Append("<a class = 'last paginate_button' " +
                                   " onclick=\"" + actionAjax + "(" + last + ",'" + sortColumn + "','" + sortDirection + "', 'search')" + "\">Trang cuối</a>");
                        str.Append("</li>");
                    }
                    str.Append("</ul>");
                    str.Append("</div>");
                }
            }
            else
            {
                str.Append("<div>");
                str.Append("Không có bản ghi nào");
                str.Append("</div>");
            }

            return new HtmlString(str.ToString());
        }

    }
}
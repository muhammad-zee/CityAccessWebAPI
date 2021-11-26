using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;

namespace Web.Services.Extensions
{
    public static class HtmlExtensions
    {
        //public static MvcHtmlString ShowBootStrapAlert(this HtmlHelper helper, string title, string text, BootStrapeAlertType alertType)
        //{

        //    string html = @"<div class=""alert " + alertType.ToDescription() + @""">
        //                  <button class=""close"" data-dismiss=""alert"">×</button>";
        //    if (!string.IsNullOrEmpty(title))
        //    {
        //        html += "<strong>" + title + @"</strong>";
        //    }
        //    html += text + "</div>";
        //    return new MvcHtmlString(html);
        //}

        //public static MvcHtmlString LabelForWithSuffix<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, object htmlAttributes)
        //{
        //    return helper.LabelFor(expression,
        //                           string.Format("{0}:",
        //                                   Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(helper.DisplayNameFor(expression).ToHtmlString().ToLower())),
        //                                   htmlAttributes);
        //}

        //public static MvcHtmlString FormattedDateTextBoxFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, object htmlAttributes)
        //{

        //    var metadata = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);
        //    if (metadata.Model != null && metadata.Model as DateTime? != null)
        //    {

        //        return helper.TextBox(metadata.PropertyName, ((DateTime)metadata.Model).FormatDateTime(), htmlAttributes);
        //    }
        //    else
        //    {
        //        return helper.TextBoxFor(expression, htmlAttributes);
        //    }
        //}

        


        //public static HtmlString RadioButtonListFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, Dictionary<string, string> dict, object htmlAttributes = null)
        //{
        //    if (htmlAttributes == null)
        //        htmlAttributes = new { @class = "form-check-input mb-2" };
        //    StringBuilder builder = new StringBuilder();

        //    if (dict != null)
        //    {
        //        foreach (var item in dict)
        //        {
        //            builder.Append(@"<div class='form-check-inline'>");
        //            builder.Append(helper.RadioButtonFor(expression, item.Key, htmlAttributes));
        //            builder.Append("<label>");
        //            builder.Append(item.Value);
        //            builder.Append("</label>");
        //            builder.Append("</div>");
        //        }
        //    }

        //    return new MvcHtmlString(builder.ToString());
        //}

    }
}

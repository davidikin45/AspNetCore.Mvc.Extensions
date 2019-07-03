using HtmlTags;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AspNetCore.Mvc.Extensions
{
    public static class HtmlHelperCheckboxOrRadioExtensions
    {
        public static Dictionary<string, string> ToDictionary(this Object attributes)
        {
            var props = attributes.GetType().GetProperties();
            var pairs = props.ToDictionary(x => x.Name.Replace("_", "-"), x => x.GetValue(attributes, null).ToString());
            return pairs;
        }

        public static Dictionary<string, string> StringsToDictionary(this Object attributes)
        {
            var props = attributes.GetType().GetProperties();
            var pairs = props.Where(x => x.PropertyType == typeof(string)).ToDictionary(x => x.Name, x => x.GetValue(attributes, null).ToString());
            return pairs;
        }
        public static HtmlString BooleanCheckbox(this IHtmlHelper htmlHelper, string expression, string text, bool isChecked, object divHtmlAttributes, object inputHtmlAttributes, object labelHtmlAttributes)
        {
            HtmlTag div = new HtmlTag("div");
            if (divHtmlAttributes != null)
            {
                foreach (var kvp in divHtmlAttributes.ToDictionary())
                {
                    div.Attr(kvp.Key, kvp.Value);
                }
            }

            var id = htmlHelper.Id(expression);

            var input = htmlHelper.CheckBox(expression, isChecked, inputHtmlAttributes).Render();

            //var checkboxHtml = htmlHelper.CheckBox(expression, isChecked, htmlAttributes).Render().Replace("true", value);

            HtmlTag label = new HtmlTag("label");
            if (labelHtmlAttributes != null)
            {
                foreach (var kvp in labelHtmlAttributes.ToDictionary())
                {
                    label.Attr(kvp.Key, kvp.Value);
                }
            }
            label.AppendHtml(text);

            div.AppendHtml(input);
            if (!string.IsNullOrEmpty(text))
            {
                div.Append(label);
            }

            return new HtmlString(div.ToString());
        }

        public static HtmlString BooleanCheckboxButton(this IHtmlHelper htmlHelper, string expression, string text, bool isChecked, object divHtmlAttributes, object inputHtmlAttributes, object labelHtmlAttributes)
        {
            HtmlTag div = new HtmlTag("div");
            if (divHtmlAttributes != null)
            {
                foreach (var kvp in divHtmlAttributes.ToDictionary())
                {
                    div.Attr(kvp.Key, kvp.Value);
                }
            }

            var input = htmlHelper.CheckBox(expression, isChecked, inputHtmlAttributes).Render();

            HtmlTag label = new HtmlTag("label");
            if (labelHtmlAttributes != null)
            {
                foreach (var kvp in labelHtmlAttributes.ToDictionary())
                {
                    label.Attr(kvp.Key, kvp.Value);
                }
            }

            if (isChecked)
            {
                label.AddClass("active");
            }

            label.AppendHtml(input);
            label.AppendHtml(text);

            div.Append(label);

            return new HtmlString(div.ToString());
        }

        public static HtmlString ValueCheckbox(this IHtmlHelper htmlHelper, string expression, string value, string text, bool isChecked, object divHtmlAttributes, object inputHtmlAttributes, object labelHtmlAttributes)
        {
            HtmlTag div = new HtmlTag("div");
            if (divHtmlAttributes != null)
            {
                foreach (var kvp in divHtmlAttributes.ToDictionary())
                {
                    div.Attr(kvp.Key, kvp.Value);
                }
            }

            var id = htmlHelper.Id(expression);
            var name = htmlHelper.Name(expression);

            HtmlTag input = new HtmlTag("input");
            if (inputHtmlAttributes != null)
            {
                foreach (var kvp in inputHtmlAttributes.ToDictionary())
                {
                    input.Attr(kvp.Key, kvp.Value);
                }
            }
            input.Id(id);
            input.Name(name);
            input.Attr("type", "checkbox");
            input.Value(value);
            if (isChecked)
            {
                input.Attr("checked", "checked");
            }

            //var checkboxHtml = htmlHelper.CheckBox(expression, isChecked, htmlAttributes).Render().Replace("true", value);

            HtmlTag label = new HtmlTag("label");
            if (labelHtmlAttributes != null)
            {
                foreach (var kvp in labelHtmlAttributes.ToDictionary())
                {
                    label.Attr(kvp.Key, kvp.Value);
                }
            }
            label.AppendHtml(text);

            div.Append(input);
            if (!string.IsNullOrEmpty(text))
            {
                div.Append(label);
            }

            return new HtmlString(div.ToString());
        }

        public static HtmlString ValueCheckboxButton(this IHtmlHelper htmlHelper, string expression, string value, string text, bool isChecked, object inputHtmlAttributes, object labelHtmlAttributes)
        {
            var id = htmlHelper.Id(expression);
            var name = htmlHelper.Name(expression);

            HtmlTag input = new HtmlTag("input");
            if (inputHtmlAttributes != null)
            {
                foreach (var kvp in inputHtmlAttributes.ToDictionary())
                {
                    input.Attr(kvp.Key, kvp.Value);
                }
            }
            input.Id(id);
            input.Name(name);
            input.Attr("type", "checkbox");
            input.Value(value);
            if (isChecked)
            {
                input.Attr("checked", "checked");
            }

            //var checkboxHtml = htmlHelper.CheckBox(expression, isChecked, htmlAttributes).Render().Replace("true", value);

            HtmlTag label = new HtmlTag("label");
            if (labelHtmlAttributes != null)
            {
                foreach (var kvp in labelHtmlAttributes.ToDictionary())
                {
                    label.Attr(kvp.Key, kvp.Value);
                }
            }
            if (isChecked)
            {
                label.AddClass("active");
            }
            label.Append(input);
            label.AppendHtml(text);

            return new HtmlString(label.ToString());
        }

        public static HtmlString ValueRadio(this IHtmlHelper htmlHelper, string expression, string value, string text, bool isChecked, object divHtmlAttributes, object inputHtmlAttributes, object labelHtmlAttributes)
        {
            // var radioHtml = htmlHelper.RadioButton(expression, value, isChecked, htmlAttributes).Render();

            HtmlTag div = new HtmlTag("div");
            if (divHtmlAttributes != null)
            {
                foreach (var kvp in divHtmlAttributes.ToDictionary())
                {
                    div.Attr(kvp.Key, kvp.Value);
                }
            }

            var input = htmlHelper.RadioButton(expression, value, isChecked, inputHtmlAttributes).Render();

            //var checkboxHtml = htmlHelper.CheckBox(expression, isChecked, htmlAttributes).Render().Replace("true", value);

            HtmlTag label = new HtmlTag("label");
            if (labelHtmlAttributes != null)
            {
                foreach (var kvp in labelHtmlAttributes.ToDictionary())
                {
                    label.Attr(kvp.Key, kvp.Value);
                }
            }
            label.AppendHtml(text);

            div.AppendHtml(input);

            if (!string.IsNullOrEmpty(text))
            {
                div.Append(label);
            }

            return new HtmlString(div.ToString());
        }

        public static HtmlString ValueRadioButton(this IHtmlHelper htmlHelper, string expression, string value, string text, bool isChecked, object inputHtmlAttributes, object labelHtmlAttributes)
        {
            var input = htmlHelper.RadioButton(expression, value, isChecked, inputHtmlAttributes).Render();

            //var checkboxHtml = htmlHelper.CheckBox(expression, isChecked, htmlAttributes).Render().Replace("true", value);

            HtmlTag label = new HtmlTag("label");
            if (labelHtmlAttributes != null)
            {
                foreach (var kvp in labelHtmlAttributes.ToDictionary())
                {
                    label.Attr(kvp.Key, kvp.Value);
                }
            }
            if (isChecked)
            {
                label.AddClass("active");
            }

            label.AppendHtml(input);
            label.AppendHtml(text);


            return new HtmlString(label.ToString());
        }

        public static HtmlString ValueCheckboxList(this IHtmlHelper htmlHelper, string expression, IEnumerable<SelectListItem> items, bool inline)
        {
            string divClass = "form-check";
            if (inline)
            {
                divClass += " form-check-inline";
            }

            var sb = new StringBuilder();
            foreach (var item in items)
            {
                sb.AppendLine(htmlHelper.ValueCheckbox(expression, item.Value, item.Text, item.Selected, new { @class = divClass }, new { @class = "form-check-input" }, new { @class = "form-check-label" }).Render());
            }
            return new HtmlString(sb.ToString());
        }

        public static HtmlString ValueRadioList(this IHtmlHelper htmlHelper, string expression, IEnumerable<SelectListItem> items, bool inline)
        {
            string divClass = "form-check";
            if (inline)
            {
                divClass += " form-check-inline";
            }

            var sb = new StringBuilder();
            foreach (var item in items)
            {
                sb.AppendLine(htmlHelper.ValueRadio(expression, item.Value, item.Text, item.Selected, new { @class = divClass }, new { @class = "form-check-input" }, new { @class = "form-check-label" }).Render());
            }
            return new HtmlString(sb.ToString());
        }

        public static HtmlString ValueCheckboxButtonList(this IHtmlHelper htmlHelper, string expression, IEnumerable<SelectListItem> items, object divHtmlAttributes, object labelHtmlAttributes)
        {
            HtmlTag div = new HtmlTag("div");
            if (divHtmlAttributes != null)
            {
                foreach (var kvp in divHtmlAttributes.ToDictionary())
                {
                    div.Attr(kvp.Key, kvp.Value);
                }
            }

            foreach (var item in items)
            {
                div.AppendHtml(htmlHelper.ValueCheckboxButton(expression, item.Value, item.Text, item.Selected, new { @class = "", autocomplete = "off" }, labelHtmlAttributes).Render());
            }

            return new HtmlString(div.ToString());
        }

        public static HtmlString ValueRadioButtonList(this IHtmlHelper htmlHelper, string expression, IEnumerable<SelectListItem> items, bool groupRadioButtons, object divHtmlAttributes, object labelHtmlAttributes)
        {
            HtmlTag div = new HtmlTag("div");
            if (divHtmlAttributes != null)
            {
                foreach (var kvp in divHtmlAttributes.ToDictionary())
                {
                    div.Attr(kvp.Key, kvp.Value);
                }
            }
            if (groupRadioButtons)
            {
                div.AddClass("btn-group");
            }

            foreach (var item in items)
            {
                div.AppendHtml(htmlHelper.ValueRadioButton(expression, item.Value, item.Text, item.Selected, new { @class = "", autocomplete = "off" }, labelHtmlAttributes).Render());
            }

            return new HtmlString(div.ToString());
        }

        public static HtmlString TrueFalseRadioButtonList(this IHtmlHelper htmlHelper, string expression, bool isChecked, bool groupRadioButtons, object divHtmlAttributes, object labelHtmlAttributes)
        {
            var items = new List<SelectListItem>();
            items.Add(new SelectListItem("True", "true", isChecked, false));
            items.Add(new SelectListItem("False", "false", !isChecked, false));

            HtmlTag div = new HtmlTag("div");
            if (divHtmlAttributes != null)
            {
                foreach (var kvp in divHtmlAttributes.ToDictionary())
                {
                    div.Attr(kvp.Key, kvp.Value);
                }
            }
            if (groupRadioButtons)
            {
                div.AddClass("btn-group");
            }

            foreach (var item in items)
            {
                div.AppendHtml(htmlHelper.ValueRadioButton(expression, item.Value, item.Text, item.Selected, new { @class = "", autocomplete = "off" }, labelHtmlAttributes).Render());
            }

            return new HtmlString(div.ToString());
        }

        public static HtmlString YesNoRadioButtonList(this IHtmlHelper htmlHelper, string expression, bool isChecked, bool groupRadioButtons, object divHtmlAttributes, object labelHtmlAttributes)
        {
            var items = new List<SelectListItem>();
            items.Add(new SelectListItem("Yes", "true", isChecked, false));
            items.Add(new SelectListItem("No", "false", !isChecked, false));

            HtmlTag div = new HtmlTag("div");
            if (divHtmlAttributes != null)
            {
                foreach (var kvp in divHtmlAttributes.ToDictionary())
                {
                    div.Attr(kvp.Key, kvp.Value);
                }
            }
            if (groupRadioButtons)
            {
                div.AddClass("btn-group");
            }

            foreach (var item in items)
            {
                div.AppendHtml(htmlHelper.ValueRadioButton(expression, item.Value, item.Text, item.Selected, new { @class = "", autocomplete = "off" }, labelHtmlAttributes).Render());
            }

            return new HtmlString(div.ToString());
        }
    }
}

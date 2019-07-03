using HtmlTags;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace AspNetCore.Mvc.Extensions
{
    public static class HtmlHelperGridExtensions
    {

        public static HtmlString Grid(this IHtmlHelper<dynamic> html, Boolean details, Boolean edit, Boolean delete, Boolean sorting, Boolean entityActions)
        {
            var div = new TagBuilder("div");
            div.AddCssClass("table-responsive");

            var table = new TagBuilder("table");
            table.AddCssClass("table");
            table.AddCssClass("table-striped");
            table.AddCssClass("table-sm");

            var thead = new TagBuilder("thead");

            var theadTr = new TagBuilder("tr");

            Dictionary<string, List<string>> actions = new Dictionary<string, List<string>>();

            //if (entityActions)
            //{

            //    var actionEvents = new ActionEvents();
            //    actions = actionEvents.GetActionsForDto(html.ViewData.ModelMetadata().ModelType);
            //    entityActions = actions.Count > 0;
            //}

            Boolean hasActions = (details || edit || delete || entityActions);

            var provider = html.ViewContext.HttpContext.RequestServices.GetRequiredService<IModelMetadataProvider>();

            foreach (var prop in html.ViewData.ModelMetadata(provider).Properties
            .Where(p => (p.ShowForDisplay && !(p.AdditionalValues.ContainsKey("ShowForGrid")) || (p.AdditionalValues.ContainsKey("ShowForGrid") && (Boolean)p.AdditionalValues["ShowForGrid"]))))
            {
                var th = new TagBuilder("th");

                var orderType = "desc";
                if (html.ViewBag.OrderColumn.ToLower() == prop.PropertyName.ToLower() && html.ViewBag.OrderType != "asc")
                {
                    orderType = "asc";
                }


                string linkText = ModelHelperExtensions.DisplayName(html.ViewData.Model, prop.PropertyName, provider).ToString();
                string link;

                Boolean enableSort = sorting;
                if (prop.AdditionalValues.ContainsKey("AllowSortForGrid"))
                {
                    enableSort = (Boolean)prop.AdditionalValues["AllowSortForGrid"];
                }

                if (enableSort)
                {
                    if (html.ViewBag.Collection != null)
                    {
                        link = html.ActionLink(linkText, "Collection", new { id = html.ViewBag.Id, collection = html.ViewBag.Collection, page = html.ViewBag.Page, pageSize = html.ViewBag.PageSize, search = html.ViewBag.Search, orderColumn = prop.PropertyName, orderType = orderType }).Render().Replace("%2F", "/");
                    }
                    else
                    {
                        link = html.ActionLink(linkText, "Index", new { page = html.ViewBag.Page, pageSize = html.ViewBag.PageSize, search = html.ViewBag.Search, orderColumn = prop.PropertyName, orderType = orderType }).Render();
                    }
                }
                else
                {
                    link = linkText;
                }

                // th.InnerHtml = ModelHelperExtensions.DisplayName(html.ViewData.Model, prop.PropertyName).ToString();
                th.InnerHtml.AppendHtml(link);

                theadTr.InnerHtml.AppendHtml(th);
            }

            if (hasActions)
            {
                var thActions = new TagBuilder("th");
                theadTr.InnerHtml.AppendHtml(thActions);
            }

            thead.InnerHtml.AppendHtml(theadTr);

            table.InnerHtml.AppendHtml(thead);

            var tbody = new TagBuilder("tbody");

            foreach (var item in html.ViewData.Model)
            {
                var tbodytr = new TagBuilder("tr");

                foreach (var prop in html.ViewData.ModelMetadata(provider).Properties
               .Where(p => (p.ShowForDisplay && !(p.AdditionalValues.ContainsKey("ShowForGrid")) || (p.AdditionalValues.ContainsKey("ShowForGrid") && (Boolean)p.AdditionalValues["ShowForGrid"]))))
                {
                    var td = new TagBuilder("td");

                    var propertyType = GetNonNullableModelType(prop);
                    var linkToCollection = propertyType.IsCollection() && prop.AdditionalValues.ContainsKey("LinkToCollectionInGrid") && (Boolean)prop.AdditionalValues["LinkToCollectionInGrid"];


                    //Folder, File, Dropdown, Repeater
                    if (prop.AdditionalValues.ContainsKey("IsDatabound"))
                    {
                        if (linkToCollection)
                        {
                            string linkText = "";
                            if (prop.AdditionalValues.ContainsKey("LinkText"))
                            {
                                linkText = (string)prop.AdditionalValues["LinkText"];
                            }

                            if (string.IsNullOrWhiteSpace(linkText))
                            {
                                linkText = ModelHelperExtensions.Display(html, item, prop.PropertyName).Render();
                            }

                            var collectionLink = html.ActionLink(linkText, "Collection", new { id = item.Id, collection = html.ViewBag.Collection != null ? html.ViewBag.Collection + "/" + prop.PropertyName.ToLower() : prop.PropertyName.ToLower() }).Render().Replace("%2F", "/");
                            HtmlContentBuilderExtensions.SetHtmlContent(td.InnerHtml, collectionLink);
                        }
                        else
                        {
                            if (prop.AdditionalValues.ContainsKey("ActionName"))
                            {
                                var linkText = (string)prop.AdditionalValues["LinkText"];
                                if (string.IsNullOrWhiteSpace(linkText))
                                {
                                    linkText = ModelHelperExtensions.Display(html, item, prop.PropertyName).Render();
                                }
                                var actionName = (string)prop.AdditionalValues["ActionName"];
                                var controllerName = (string)prop.AdditionalValues["ControllerName"];

                                RouteValueDictionary routeValues = null;
                                if (prop.AdditionalValues.ContainsKey("RouteValueDictionary"))
                                {
                                    routeValues = (RouteValueDictionary)prop.AdditionalValues["RouteValueDictionary"];
                                    SetRouteValues(html, item, routeValues);
                                }

                                string collectionLink = "";
                                if (routeValues == null)
                                {
                                    collectionLink = html.ActionLink(linkText, actionName, controllerName).Render().Replace("%2F", "/");
                                }
                                else
                                {
                                    collectionLink = html.ActionLink(linkText, actionName, controllerName, routeValues).Render().Replace("%2F", "/");
                                }

                                HtmlContentBuilderExtensions.SetHtmlContent(td.InnerHtml, collectionLink);
                            }
                            else
                            {
                                HtmlContentBuilderExtensions.SetHtmlContent(td.InnerHtml, ModelHelperExtensions.Display(html, item, prop.PropertyName));
                            }
                        }
                    }
                    else if (prop.ModelType == typeof(FileInfo))
                    {
                        HtmlContentBuilderExtensions.SetHtmlContent(td.InnerHtml, ModelHelperExtensions.Display(html, item, prop.PropertyName));
                    }
                    else if (prop.ModelType == typeof(Point))
                    {
                        var model = (Point)item.GetType().GetProperty(prop.PropertyName).GetValue(item, null);
                        if (model != null && model.Y != 0 && model.X != 0)
                        {
                            string value = model.Y.ToString("G", CultureInfo.InvariantCulture) + "," + model.X.ToString("G", CultureInfo.InvariantCulture);
                            td.InnerHtml.Append(value);
                        }
                    }
                    else
                    {
                        //String
                        if (linkToCollection)
                        {
                            string linkText = "";
                            if (prop.AdditionalValues.ContainsKey("LinkText"))
                            {
                                linkText = (string)prop.AdditionalValues["LinkText"];
                            }

                            if (string.IsNullOrWhiteSpace(linkText))
                            {
                                linkText = ModelHelperExtensions.DisplayTextSimple(html, item, prop.PropertyName).ToString();
                            }
                            var collectionLink = html.ActionLink(linkText, "Collection", new { id = item.Id, collection = html.ViewBag.Collection != null ? html.ViewBag.Collection + "/" + prop.PropertyName.ToLower() : prop.PropertyName.ToLower() }).Render().Replace("%2F", "/");
                            HtmlContentBuilderExtensions.SetHtmlContent(td.InnerHtml, collectionLink);
                        }
                        else
                        {
                            string value = ModelHelperExtensions.DisplayTextSimple(html, item, prop.PropertyName).ToString();
                            td.InnerHtml.Append(value.Truncate(70));
                        }
                    }

                    tbodytr.InnerHtml.AppendHtml(td);
                }

                if (hasActions)
                {
                    var tdActions = new TagBuilder("td");

                    if (entityActions)
                    {
                        var postUrl = html.Url().Action("TriggerAction", new { id = item.Id });

                        tdActions.InnerHtml.AppendHtml("<div class='btn-group mr-2 mb-2'>");
                        tdActions.InnerHtml.AppendHtml("<form action ='" + postUrl + "' method='POST' />");

                        tdActions.InnerHtml.AppendHtml("<button type='button' class='btn btn-secondary btn-sm dropdown-toggle' data-toggle='dropdown' aria-haspopup='true' aria-expanded='false'>");
                        tdActions.InnerHtml.AppendHtml("Actions");
                        tdActions.InnerHtml.AppendHtml("</button>");

                        tdActions.InnerHtml.AppendHtml("<div class='dropdown-menu'>");

                        foreach (var action in actions)
                        {
                            foreach (var actionDescription in action.Value)
                            {
                                var button = "<button type='submit' class='dropdown-item' name='action' value='" + action.Key + "'>" + actionDescription + "</button>";
                                tdActions.InnerHtml.AppendHtml(button);
                            }
                        }

                        tdActions.InnerHtml.AppendHtml(@"</div>");
                        tdActions.InnerHtml.AppendHtml(@"</form>");
                        tdActions.InnerHtml.AppendHtml(@"</div>");
                    }

                    if (details)
                    {
                        if (html.ViewBag.Collection != null)
                        {
                            tdActions.InnerHtml.AppendHtml(html.IconLink("Details", "Collection", new { id = html.ViewBag.Id, collection = html.ViewBag.Collection + "/" + item.Id }, "fa fa-search", new { @class = "btn btn-primary btn-sm mr-2 mb-2" }));
                        }
                        else
                        {
                            tdActions.InnerHtml.AppendHtml(html.IconLink("Details", "Details", new { id = item.Id }, "fa fa-search", new { @class = "btn btn-primary btn-sm mr-2 mb-2" }));
                        }
                    }

                    if (edit)
                    {
                        tdActions.InnerHtml.AppendHtml(html.IconLink("Edit", "Edit", new { id = item.Id }, "fa fa-pencil", new { @class = "btn btn-warning btn-sm mr-2 mb-2" }));
                    }

                    if (delete)
                    {
                        tdActions.InnerHtml.AppendHtml(html.IconLink("Delete", "Delete", new { id = item.Id }, "fa fa-trash", new { @class = "btn btn-danger btn-sm mr-2 mb-2" }));
                    }

                    tbodytr.InnerHtml.AppendHtml(tdActions);
                }

                tbody.InnerHtml.AppendHtml(tbodytr);
            }

            table.InnerHtml.AppendHtml(tbody);

            div.InnerHtml.AppendHtml(table);

            return new HtmlString(div.Render());
        }

        private static Type GetNonNullableModelType(Microsoft.AspNetCore.Mvc.ModelBinding.ModelMetadata modelMetadata)
        {
            Type realModelType = modelMetadata.ModelType;
            Type underlyingType = Nullable.GetUnderlyingType(realModelType);


            if (underlyingType != null)
            {
                realModelType = underlyingType;
            }

            return realModelType;
        }

        private static void SetRouteValues(IHtmlHelper htmlHelper, dynamic obj, RouteValueDictionary routeValues)
        {
            if (routeValues == null)
                return;

            foreach (var kvp in routeValues.Where(kvp => kvp.Value.GetType() == typeof(string)))
            {
                string value = GetObjectProperty(htmlHelper, obj, kvp.Value.ToString());
                routeValues[kvp.Key] = value;
            }
        }

        private static string GetObjectProperty(IHtmlHelper htmlHelper, dynamic obj, string displayExpression)
        {
            string value = displayExpression;

            if (!value.Contains("{") && !value.Contains(" "))
            {
                value = "{" + value + "}";
            }

            var replacementTokens = GetReplacementTokens(value);
            foreach (var token in replacementTokens)
            {
                var propertyName = token.Substring(1, token.Length - 2);
                if (ObjectExtensions.DynamicHasProperty(obj, propertyName))
                {
                    var displayString = ObjectExtensions.GetPropValue(obj, propertyName) != null ? ObjectExtensions.GetPropValue(obj, propertyName).ToString() : "";
                    value = value.Replace(token, displayString);
                }
                else
                {
                    value = value.Replace(token, propertyName);
                }
            }

            return value;
        }

        private static List<String> GetReplacementTokens(String str)
        {
            Regex regex = new Regex(@"{(.*?)}", RegexOptions.IgnoreCase);
            MatchCollection matches = regex.Matches(str);

            // Results include braces (undesirable)
            return matches.Cast<Match>().Select(m => m.Value).Distinct().ToList();
        }
    }
}

# ASP.NET Core MVC Extensions

[![nuget](https://img.shields.io/nuget/v/AspNetCore.Mvc.Extensions.svg)](https://www.nuget.org/packages/AspNetCore.Mvc.Extensions/)  ![Downloads](https://img.shields.io/nuget/dt/AspNetCore.Mvc.Extensions.svg "Downloads")

## Installation

### NuGet
```
PM> Install-Package AspNetCore.Mvc.Extensions
```

### .Net CLI
```
> dotnet add package AspNetCore.Mvc.Extensions
```

## Usage

```
services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
.AddMvcDisplayConventions(
new AppendAsterixToRequiredFieldLabels(),
new HtmlByNameConventionFilter(),
new LabelTextConventionFilter(),
new TextAreaByNameConventionFilter(),
new TextboxPlaceholderConventionFilter(),
new DisableConvertEmptyStringToNull())
.AddMvcValidationConventions()
.AddMvcDisplayAttributes()
.AddMvcInheritanceValidationAttributeAdapterProvider()
.AddMvcViewRenderer();

services.AddFluentMetadata();
```

## Display Conventions
* IDisplayConventionFilter classes transform the display metadata by convention.
```
services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
.AddMvcDisplayConventions(
new AppendAsterixToRequiredFieldLabels(),
new HtmlByNameConventionFilter(),
new LabelTextConventionFilter(),
new TextAreaByNameConventionFilter(),
new TextboxPlaceholderConventionFilter(),
new DisableConvertEmptyStringToNull());
```

| Display Convention                 | Description                                                                                     |
|:-----------------------------------|:------------------------------------------------------------------------------------------------|
| AppendAsterixToRequiredFieldLabels | Appends \* to label automatically                                                          	   |
| HtmlByNameConventionFilter         | If the field name contains 'html' DataTypeName will be set to 'Html'                            |
| LabelTextConventionFilter          | If a display attribute name is not set on model property FirstName would become 'First Name'    |
| TextAreaByNameConventionFilter     | If the field name contains 'body' or 'comments' DataTypeName will be set to 'MultilineText'     |
| TextboxPlaceholderConventionFilter | If Display attribute prompt is not set on model property FirstName would become 'First Name...' |
| DisableConvertEmptyStringToNull    | By default MVC converts empty string to Null, this disables the convention                      |


## Validation Conventions
* IValidationConventionFilter classes transform the validation metadata by convention.
```
services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
.AddMvcValidationConventions();
```

## Display Attributes
* IDisplayMetadataAttribute classes are a good way to define new editor/display types or pass additional information for existing via AdditionalValues.
```
services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
.AddMvcDisplayAttributes();
```

* SliderAttribute
```
public class SliderAttribute : Attribute, IDisplayMetadataAttribute
{
	public int Min { get; set; } = 0;
	public int Max { get; set; } = 100;

	public SliderAttribute()
	{

	}

	public SliderAttribute(int min, int max)
	{
		Min = min;
		Max = max;
	}

	public void TransformMetadata(DisplayMetadataProviderContext context)
	{
		var propertyAttributes = context.Attributes;
		var modelMetadata = context.DisplayMetadata;
		var propertyName = context.Key.Name;

		modelMetadata.DataTypeName = "Slider";
		modelMetadata.AdditionalValues["Min"] = Min;
		modelMetadata.AdditionalValues["Max"] = Max;
	}
}
```

* View Model
```
public class ViewModel
{
	[Slider(0, 200)]
	public int Value {get; set;}
}
```

* Views\Shared\EditorTemplates\Slider.cshtml
```
@model Int32?
@Html.TextBox("", ViewData.TemplateInfo.FormattedModelValue,
        new { @class = "custom-range", @type = "range", min = (int)ViewData.ModelMetadata.AdditionalValues["Min"], max = (int)ViewData.ModelMetadata.AdditionalValues["Max"] })
```

* Views\Shared\DisplayTemplates\Slider.cshtml
```
@model int?
@{ 
    var value = Html.ViewData.TemplateInfo.FormattedModelValue;
    var displayValue = "";
    if (value != null)
    {
        displayValue = value.ToString();
    }
}
@Html.Raw(displayValue)
```

## Validation Attribute Inheritance
* By default the [ValidationAttributeAdapterProdiver](https://github.com/aspnet/AspNetCore/blob/c565386a3ed135560bc2e9017aa54a950b4e35dd/src/Mvc/Mvc.DataAnnotations/src/ValidationAttributeAdapterProvider.cs) doesn't perform client side validation if you inherit from an existing validation attribute.
* The below service collection extensions overrides the existing IValidationAttributeAdapterProvider enabling client side validation for inherited types. 
```
services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
.AddMvcInheritanceValidationAttributeAdapterProvider();
```

| Attribute                  |
|:---------------------------|
| RegularExpressionAttribute |
| MaxLengthAttribute         |
| RequiredAttribute          |
| CompareAttribute           |
| MinLengthAttribute         |
| CreditCardAttribute        |
| StringLengthAttribute      |
| RangeAttribute             |
| EmailAddressAttribute      |
| PhoneAttribute             |
| UrlAttribute               |
| FileExtensionsAttribute    |


* Example
```
public class NumberValidatorAttribute : RangeAttribute
{
	public NumberValidatorAttribute()
		:base(0, int.MaxValue)
	{
		//The field {0} must be between {1} and {2}.
		ErrorMessage = "The field {0} must be a number.";
	}
}
```

## Render Razor Views as Html
* Gives the ability to render Views, Partials, Display for Model or Editor for Model. 
* Useful for using Razor to generate PDFs.
```
services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
.AddMvcViewRenderer();
```

## Fluent Metadata
* Allows modelmetadata to be configured via fluent syntax rather than attributes.
```
services.AddFluentMetadata();
```

* Example
```
public class PersonConfig : ModelMetadataConfiguration<Person>
{
	public PersonConfig()
	{
		Configure(p => p.Name).Required();
		Configure<string>("Name").Required();
	}
}
```

## Delimited Query Strings
```
[DelimitedQueryString(',', '|')]
public async Task<ActionResult<List<Model>>> BulkGetByIds(IEnumerable<string> ids)
{

}
```

## Routing Attributes

| Attribute                                  | Description                                                              |
|:-------------------------------------------|:-------------------------------------------------------------------------|
| AcceptHeaderMatchesMediaTypeAttribute      | Action only executed if Accept Header matches                            |
| ContentTypeHeaderMatchesMediaTypeAttribute | Action only executed if Content-Type Header matches                      |
| RequestHeaderMatchesMediaTypeAttribute     | Action only executed if header matches media type                        |
| AjaxRequestAttribute                       | Action only executed if X-Requested-With matches 'XMLHttpRequest'        |
| NoAjaxRequestAttribute                     | Action only executed if X-Requested-With does not match 'XMLHttpRequest' |

```
[HttpPost()]
[Consumes("application/json", "application/vnd.app.bookforcreation+json")]
[RequestHeaderMatchesMediaType(HeaderNames.ContentType,
	"application/json",  "application/vnd.app.bookforcreation+json")]
[ProducesResponseType(StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status422UnprocessableEntity, 
	Type = typeof(Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary))]
public async Task<ActionResult<Book>> CreateBook(
	Guid authorId,
	[FromBody] BookForCreation bookForCreation)
{
	if (!await _authorRepository.AuthorExistsAsync(authorId))
	{
		return NotFound();
	}

	var bookToAdd = _mapper.Map<Entities.Book>(bookForCreation);
	_bookRepository.AddBook(bookToAdd);
	await _bookRepository.SaveChangesAsync();

	return CreatedAtRoute(
		"GetBook",
		new { authorId, bookId = bookToAdd.Id },
		_mapper.Map<Book>(bookToAdd));
}

[HttpPost()]
[Consumes("application/vnd.app.bookforcreationwithamountofpages+json")]
[RequestHeaderMatchesMediaType(HeaderNames.ContentType,
	"application/vnd.app.bookforcreationwithamountofpages+json")]
[ProducesResponseType(StatusCodes.Status201Created)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesResponseType(StatusCodes.Status422UnprocessableEntity,
	Type = typeof(Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary))]
[ApiExplorerSettings(IgnoreApi = true)]
public async Task<ActionResult<Book>> CreateBookWithAmountOfPages(
	Guid authorId,
	[FromBody] BookForCreationWithAmountOfPages bookForCreationWithAmountOfPages)
{
	if (!await _authorRepository.AuthorExistsAsync(authorId))
	{
		return NotFound();
	}

	var bookToAdd = _mapper.Map<Entities.Book>(bookForCreationWithAmountOfPages);
	_bookRepository.AddBook(bookToAdd);
	await _bookRepository.SaveChangesAsync();

	return CreatedAtRoute(
		"GetBook",
		new { authorId, bookId = bookToAdd.Id },
		_mapper.Map<Book>(bookToAdd));
}
```

## Action Results

| ActionResult     | Description                    |
|:-----------------|:-------------------------------|
| CsvResult        | Returns CSV from IEnumerable   |
| HtmlResult       | Returns text/html              |
| JavaScriptResult | Returns application/javascript |

## Validation Attributes

| Attribute                    | Description                                                                      |
|:-----------------------------|:---------------------------------------------------------------------------------|
| AutoModelValidationAttribute | Checks ModelState.IsValid and returns view with with 400 status code if invalid. |
| ValidatableAttribute         | if request contains x-action-intent:validate header only model validation occurs |

```
services.AddMvc(options =>
{
	options.Filters.Add<ValidatableAttribute>();
	options.Filters.Add<AutoModelValidationAttribute>();
});
```

## Exception Filter Attributes

| Attribute                    | Description                                                                      |
|:-----------------------------|:---------------------------------------------------------------------------------|
| OperationCancelledExceptionFilterAttribute | Catches operation cancelled exceptions. |

```
services.AddMvc(options =>
{
	options.Filters.Add<OperationCancelledExceptionFilterAttribute>();
});
```

## Get All Routes ASP.NET Core 2.2
```
app.UseMvc(routes =>
{
	routes.MapAllRoutes("/all-routes");
});
```

## Get All Routes ASP.NET Core 3.0
```
app.UseEndpoints(endpoints =>
{
	endpoints.MapAllRoutes("/all-routes");
}
```

## Localization ASP.NET Core 2.2
* [Endpoint Routing](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-2.2) (enabled by default from 2.2 onwards) changes how links are generated. Previously all ambient route data was passed to action links. Now ambient route data is only reused if generating link for the same controller/action. I have extended the UrlHelper implementation so route keys can be reqgistered as being globally ambient.
* https://andrewlock.net/url-culture-provider-using-middleware-as-mvc-filter-in-asp-net-core-1-1-0/
* https://andrewlock.net/applying-the-routedatarequest-cultureprovider-globally-with-middleware-as-filters/
* https://andrewlock.net/using-a-culture-constraint-and-catching-404s-with-the-url-culture-provider/
* https://andrewlock.net/redirecting-unknown-cultures-to-the-default-culture-when-using-the-url-culture-provider/

```
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public bool AlwaysIncludeCultureInUrl { get; set; } = true;

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<CookiePolicyOptions>(options =>
        {
            // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            options.CheckConsentNeeded = context => true;
            options.MinimumSameSitePolicy = SameSiteMode.None;
        });

        services.AddLocalization(options => options.ResourcesPath = "Resources");

        services.AddMvc(options =>
        {
            //https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-2.2
            //https://github.com/aspnet/AspNetCore/blob/1c126ab773059d6a5899fc29547cb86ed49c46bf/src/Http/Routing/src/Template/TemplateBinder.cs
            //EnableEndpointRouting = false Ambient route values always reused.
            //EnableEndpointRouting = true. Ambient route values only reused if generating link for same controller/action. 

            options.EnableEndpointRouting = true;

            if (AlwaysIncludeCultureInUrl)
                options.AddCultureRouteConvention("cultureCheck");
            else
                options.AddOptionalCultureRouteConvention("cultureCheck");

            //Middleware Pipeline - Wraps MVC
            options.Filters.Add(new MiddlewareFilterAttribute(typeof(LocalizationPipeline)));

        }).SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
            .AddDataAnnotationsLocalization()
            //If EnableEndpointRouting is enabled (enabled by default from 2.2) ambient route data is required. 
            .AddAmbientRouteDataUrlHelperFactory(options => {
                options.AmbientRouteDataKeys.Add(new AmbientRouteData("area", false));
                options.AmbientRouteDataKeys.Add(new AmbientRouteData("culture", true));
                options.AmbientRouteDataKeys.Add(new AmbientRouteData("ui-culture", true));
            });

        services.AddCultureRouteConstraint("cultureCheck");

        services.AddRequestLocalizationOptions(
            defaultCulture: "en-AU",
            supportAllCountryFormatting: false,
            supportAllLanguagesFormatting: false,
            supportUICultureFormatting: true,
            allowDefaultCultureLanguage: true,
            supportedUICultures: "en");

        services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostingEnvironment env, IOptions<RequestLocalizationOptions> localizationOptions)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseRequestLocalization(localizationOptions.Value);

        app.UseStaticFiles();
        app.UseCookiePolicy();

        app.UseMvc(routes =>
        {


        });

        var routeBuilder = new RouteBuilder(app);

        if (AlwaysIncludeCultureInUrl)
        {
            routeBuilder.RedirectCulturelessToDefaultCulture(localizationOptions.Value);
        }

        app.UseRouter(routeBuilder.Build());
    }
}
```

## Localization ASP.NET Core 3.0
* [Endpoint Routing](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-2.2) (enabled by default from 2.2 onwards) changes how links are generated. Previously all ambient route data was passed to action links. Now ambient route data is only reused if generating link for the same controller/action. I have extended the UrlHelper implementation so route keys can be reqgistered as being globally ambient.
* https://andrewlock.net/url-culture-provider-using-middleware-as-mvc-filter-in-asp-net-core-1-1-0/
* https://andrewlock.net/applying-the-routedatarequest-cultureprovider-globally-with-middleware-as-filters/
* https://andrewlock.net/using-a-culture-constraint-and-catching-404s-with-the-url-culture-provider/
* https://andrewlock.net/redirecting-unknown-cultures-to-the-default-culture-when-using-the-url-culture-provider/

```
 public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    public bool AlwaysIncludeCultureInUrl { get; set; } = true;
    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.Configure<CookiePolicyOptions>(options =>
        {
            // This lambda determines whether user consent for non-essential cookies is needed for a given request.
            options.CheckConsentNeeded = context => true;
        });

        services.AddControllersWithViews(options =>
        {
            options.EnableEndpointRouting = true;

            if (AlwaysIncludeCultureInUrl)
                options.AddCultureRouteConvention("cultureCheck");
            else
                options.AddOptionalCultureRouteConvention("cultureCheck");

            //Middleware Pipeline - Wraps MVC
            options.Filters.Add(new MiddlewareFilterAttribute(typeof(LocalizationPipeline)));
        }).SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
            .AddDataAnnotationsLocalization()
            //If EnableEndpointRouting is enabled (enabled by default from 2.2) ambient route data is required. 
            .AddAmbientRouteDataUrlHelperFactory(options =>
            {
                options.AmbientRouteDataKeys.Add(new AmbientRouteData("area", false));
                options.AmbientRouteDataKeys.Add(new AmbientRouteData("culture", true));
                options.AmbientRouteDataKeys.Add(new AmbientRouteData("ui-culture", true));
            });

        services.AddRazorPages();

        services.AddCultureRouteConstraint("cultureCheck");

        services.AddRequestLocalizationOptions(
            defaultCulture: "en-AU",
            supportAllCountryFormatting: false,
            supportAllLanguagesFormatting: false,
            supportUICultureFormatting: true,
            allowDefaultCultureLanguage: true,
            supportedUICultures: "en");

        services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<RequestLocalizationOptions> localizationOptions)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseRequestLocalization(localizationOptions.Value);

        app.UseStaticFiles();

        app.UseCookiePolicy();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapRazorPages();

            if (AlwaysIncludeCultureInUrl)
            {
                endpoints.Map("{culture:cultureCheck}/{*path}", ctx =>
                {
                    ctx.Response.StatusCode = StatusCodes.Status404NotFound;
                    return Task.CompletedTask;
                });

                //redirect culture-less routes
                endpoints.Map("{*path}", (RequestDelegate)(ctx =>
                {
                    var defaultCulture = localizationOptions.Value.DefaultRequestCulture.Culture.Name;

                    var cultureFeature = ctx.Features.Get<IRequestCultureFeature>();
                    var actualCulture = cultureFeature?.RequestCulture.Culture.Name;
                    var actualCultureLanguage = cultureFeature?.RequestCulture.Culture.TwoLetterISOLanguageName;

                    var path = ctx.GetRouteValue("path") ?? string.Empty;
                    var culturedPath = $"{ctx.Request.PathBase}/{actualCulture}/{path}{ctx.Request.QueryString.ToString()}";
                    ctx.Response.Redirect(culturedPath);
                    return Task.CompletedTask;
                }));
            }
        });
    }
}
```

## Authors

* **Dave Ikin** - [davidikin45](https://github.com/davidikin45)


## License

This project is licensed under the MIT License
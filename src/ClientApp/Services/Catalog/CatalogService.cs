using eShop.ClientApp.Helpers;
using eShop.ClientApp.Models.Catalog;
using eShop.ClientApp.Services.FixUri;
using eShop.ClientApp.Services.RequestProvider;
using eShop.ClientApp.Services.Settings;

namespace eShop.ClientApp.Services.Catalog;

/// <summary>
/// Service für den Zugriff auf Katalogdaten.
/// </summary>
public class CatalogService : ICatalogService
{
    private const string ApiUrlBase = "api/catalog";
    private const string ApiVersion = "api-version=2.0";

    private readonly IFixUriService _fixUriService;
    private readonly IRequestProvider _requestProvider;
    private readonly ISettingsService _settingsService;

    /// <summary>
    /// Erstellt eine neue Instanz des <see cref="CatalogService"/>.
    /// </summary>
    /// <param name="settingsService">Service für Anwendungseinstellungen.</param>
    /// <param name="requestProvider">Provider für HTTP-Anfragen.</param>
    /// <param name="fixUriService">Service zum Korrigieren von Bild-URIs.</param>
    public CatalogService(ISettingsService settingsService, IRequestProvider requestProvider,
        IFixUriService fixUriService)
    {
        _settingsService = settingsService;
        _requestProvider = requestProvider;
        _fixUriService = fixUriService;
    }

    /// <summary>
    /// Filtert Katalogartikel nach Marke und Typ.
    /// </summary>
    /// <param name="catalogBrandId">ID der Marke.</param>
    /// <param name="catalogTypeId">ID des Typs.</param>
    /// <returns>Gefilterte Liste von <see cref="CatalogItem"/>.</returns>
    public async Task<IEnumerable<CatalogItem>> FilterAsync(int catalogBrandId, int catalogTypeId)
    {
        var uri = UriHelper.CombineUri(_settingsService.GatewayCatalogEndpointBase,
            $"{ApiUrlBase}//items?type={catalogTypeId}&brand={catalogBrandId}&PageSize=100&PageIndex=0&{ApiVersion}");

        var catalog = await _requestProvider.GetAsync<CatalogRoot>(uri).ConfigureAwait(false);

        return catalog?.Data ?? Enumerable.Empty<CatalogItem>();
    }

    /// <summary>
    /// Ruft alle Katalogartikel ab.
    /// </summary>
    /// <returns>Liste von <see cref="CatalogItem"/>.</returns>
    public async Task<IEnumerable<CatalogItem>> GetCatalogAsync()
    {
        var uri = UriHelper.CombineUri(_settingsService.GatewayCatalogEndpointBase, $"{ApiUrlBase}/items?PageSize=100&{ApiVersion}");

        var catalog = await _requestProvider.GetAsync<CatalogRoot>(uri).ConfigureAwait(false);

        if (catalog?.Data != null)
        {
            _fixUriService.FixCatalogItemPictureUri(catalog.Data);
            return catalog.Data;
        }

        return Enumerable.Empty<CatalogItem>();
    }

    /// <summary>
    /// Ruft einen bestimmten Katalogartikel anhand der ID ab.
    /// </summary>
    /// <param name="catalogItemId">ID des Katalogartikels.</param>
    /// <returns>Der <see cref="CatalogItem"/> oder null.</returns>
    public async Task<CatalogItem> GetCatalogItemAsync(int catalogItemId)
    {
        var uri = UriHelper.CombineUri(_settingsService.GatewayCatalogEndpointBase,
            $"{ApiUrlBase}/items/{catalogItemId}?{ApiVersion}");

        var catalogItem = await _requestProvider.GetAsync<CatalogItem>(uri).ConfigureAwait(false);

        if (catalogItem != null)
        {
            _fixUriService.FixCatalogItemPictureUri(new[] {catalogItem});
            return catalogItem;
        }

        return default;
    }

    /// <summary>
    /// Ruft alle verfügbaren Marken ab.
    /// </summary>
    /// <returns>Liste von <see cref="CatalogBrand"/>.</returns>
    public async Task<IEnumerable<CatalogBrand>> GetCatalogBrandAsync()
    {
        var uri = UriHelper.CombineUri(_settingsService.GatewayCatalogEndpointBase, $"{ApiUrlBase}/catalogbrands?{ApiVersion}");

        var brands = await _requestProvider.GetAsync<IEnumerable<CatalogBrand>>(uri).ConfigureAwait(false);

        return brands?.ToArray() ?? Enumerable.Empty<CatalogBrand>();
    }

    /// <summary>
    /// Ruft alle verfügbaren Typen ab.
    /// </summary>
    /// <returns>Liste von <see cref="CatalogType"/>.</returns>
    public async Task<IEnumerable<CatalogType>> GetCatalogTypeAsync()
    {
        var uri = UriHelper.CombineUri(_settingsService.GatewayCatalogEndpointBase, $"{ApiUrlBase}/catalogtypes?{ApiVersion}");

        var types = await _requestProvider.GetAsync<IEnumerable<CatalogType>>(uri).ConfigureAwait(false);

        return types?.ToArray() ?? Enumerable.Empty<CatalogType>();
    }
}

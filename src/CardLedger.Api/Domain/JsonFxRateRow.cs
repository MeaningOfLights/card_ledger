using System.Text.Json.Serialization;

namespace CardLedger.Api.Domain
{
    /// <summary>
    /// Data row representing a Fx Rate in a static json file (mimicking the downloads here https://fiscaldata.treasury.gov/datasets/treasury-reporting-rates-exchange/treasury-reporting-rates-of-exchange)
    /// </summary>
    internal sealed class JsonFxRateRow
    {
        /// <summary>
        /// Gets or sets the currency.
        /// </summary>
        [JsonInclude]
        internal string? Currency { get; set; }

        /// <summary>
        /// Gets or sets the rate date.
        /// </summary>
        [JsonInclude]
        internal string? RateDate { get; set; }

        /// <summary>
        /// Gets or sets the usd to currency.
        /// </summary>
        [JsonInclude]
        internal decimal UsdToCurrency { get; set; }
    }
}


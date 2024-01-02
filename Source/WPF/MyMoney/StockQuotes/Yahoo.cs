﻿using ModernWpf.Controls;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Walkabout.StockQuotes
{
    /// <summary>
    /// This class encapsulates the REST API on the https://query2.finance.yahoo.com/ stock service.
    /// </summary>
    internal class YahooFinance : ThrottledStockQuoteService
    {
        private static readonly string name = "Yahoo";
        private static readonly string baseAddress = "https://query2.finance.yahoo.com/v8/finance/chart/";
        // {0}=symbol
        private const string stockQuoteUri = "https://query2.finance.yahoo.com/v8/finance/chart/{0}?interval={1}&range={2}";
        private HashSet<string> symbolsNotFound = new HashSet<string>();

        private string[] validRanges = {
                        "1d",
                        "5d",
                        "1mo",
                        "3mo",
                        "6mo",
                        "1y",
                        "2y",
                        "5y",
                        "10y",
                        "max"
        };
        private TimeSpan[] validRangeSpans = {
            TimeSpan.FromDays(1),
            TimeSpan.FromDays(5),
            TimeSpan.FromDays(30),
            TimeSpan.FromDays(30 * 3),
            TimeSpan.FromDays(30 * 6),
            TimeSpan.FromDays(365),
            TimeSpan.FromDays(365 * 2),
            TimeSpan.FromDays(365 * 5),
            TimeSpan.FromDays(365 * 10),
            TimeSpan.FromDays(365 * 100),
        };

        public YahooFinance(StockServiceSettings settings, string logPath) : base(settings, logPath)
        {
            settings.Name = name;
        }

        public override string FriendlyName => name;

        public override string WebAddress => baseAddress;

        public override bool SupportsHistory => true;

        public static StockServiceSettings GetDefaultSettings()
        {
            return new StockServiceSettings()
            {
                Name = name,
                Address = baseAddress,
                ApiKey = "",
                ApiRequestsPerMinuteLimit = 60,
                ApiRequestsPerDayLimit = 0,
                ApiRequestsPerMonthLimit = 0,
                HistoryEnabled = true
            };
        }

        public static bool IsMySettings(StockServiceSettings settings)
        {
            return settings.Name == name;
        }

        protected override async Task<StockQuote> DownloadThrottledQuoteAsync(string symbol)
        {
            var list = await this.DownloadChart(symbol, "1d");
            if (list.Count > 0)
            {
                var quote = list[0];
                if (string.Compare(quote.Symbol, symbol, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    throw new StockQuoteNotFoundException(string.Format(Walkabout.Properties.Resources.DifferentSymbolReturned, symbol, quote.Symbol));
                }
                else
                {
                    return quote;
                }
            }

            throw new StockQuoteNotFoundException(symbol);
        }

        private async Task<List<StockQuote>> DownloadChart(string symbol, string range) 
        { 
            if (this.symbolsNotFound.Contains(symbol))
            {
                throw new StockQuoteNotFoundException(symbol);
            }

            Debug.WriteLine($"Yahoo: DownloadThrottledQuoteAsync {symbol}");
            await Task.Delay(1000);

            string uri = string.Format(stockQuoteUri, symbol, "1d", range);
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            client.Timeout = TimeSpan.FromSeconds(30);
            var msg = await client.GetAsync(uri, this.TokenSource.Token);
            if (!msg.IsSuccessStatusCode)
            {
                if (msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    this.symbolsNotFound.Add(symbol);
                    throw new StockQuoteNotFoundException(symbol);
                }
                throw new Exception(this.FriendlyName + " http error " + msg.StatusCode + ": " + msg.ReasonPhrase);
            }
            else
            {
                this.CountCall();
                using (Stream stm = await msg.Content.ReadAsStreamAsync())
                {
                    using (StreamReader sr = new StreamReader(stm, Encoding.UTF8))
                    {
                        string json = sr.ReadToEnd();
                        JObject o = JObject.Parse(json);                        
                        return this.ParseStockQuotes(o);
                    }
                }
            }
        }

        private List<StockQuote> ParseStockQuotes(JObject child)
        {
            // See https://query2.finance.yahoo.com/v8/finance/chart/MSFT?interval=1d&range=5d
            var list = new List<StockQuote>();

            string symbol = null;
            JToken value;
            if (child.TryGetValue("chart", StringComparison.Ordinal, out value) && value.Type == JTokenType.Object)
            {
                JObject chart = (JObject)value;
                if (chart.TryGetValue("result", StringComparison.Ordinal, out value) && value.Type == JTokenType.Array)
                {
                    JArray result = (JArray)value;
                    if (result.First.Type == JTokenType.Object)
                    {
                        JObject item = (JObject)result.First;
                        if (item.TryGetValue("meta", StringComparison.Ordinal, out value) && value.Type == JTokenType.Object)
                        {
                            symbol = (string)value["symbol"];
                            // TBD: check currency
                            // TBD: check instrumentType=EQUITY
                        }

                        if (item.TryGetValue("timestamp", StringComparison.Ordinal, out value) && value.Type == JTokenType.Array)
                        {
                            foreach (JToken timestamp in (JArray)value)
                            {
                                // this is where we find multiple values if the range > 1d.
                                long ticks = (long)timestamp;
                                var quote = new StockQuote() { 
                                    Symbol = symbol,
                                    Downloaded = DateTime.Now, 
                                    Date = DateTimeOffset.FromUnixTimeSeconds(ticks).LocalDateTime 
                                };
                                list.Add(quote);
                            }
                        }

                        if (item.TryGetValue("indicators", StringComparison.Ordinal, out value) && value.Type == JTokenType.Object)
                        {
                            JObject indicators = (JObject)value;
                            if (indicators.TryGetValue("quote", StringComparison.Ordinal, out value) && value.Type == JTokenType.Array)
                            {
                                JArray quotes = (JArray)value;
                                if (quotes.First.Type == JTokenType.Object)
                                {
                                    JObject quote = (JObject)(quotes.First);
                                    if (quote.TryGetValue("open", StringComparison.Ordinal, out value) && value.Type == JTokenType.Array)
                                    {
                                        // this is where we find multiple values if the range > 1d.
                                        int index = 0;
                                        foreach (JToken number in (JArray)value)
                                        {
                                            if (number.Type == JTokenType.Float || number.Type == JTokenType.Integer)
                                            {
                                                list[index].Open = (decimal)number;
                                            }
                                            index++;
                                        }
                                    }

                                    if (quote.TryGetValue("low", StringComparison.Ordinal, out value) && value.Type == JTokenType.Array)
                                    {
                                        // this is where we find multiple values if the range > 1d.
                                        if (value.First.Type == JTokenType.Float)
                                        {
                                            int index = 0;
                                            foreach (JToken number in (JArray)value)
                                            {
                                                if (number.Type == JTokenType.Float || number.Type == JTokenType.Integer)
                                                {
                                                    list[index].Low = (decimal)number;
                                                }
                                                index++;
                                            }
                                        }
                                    }
                                    if (quote.TryGetValue("volume", StringComparison.Ordinal, out value) && value.Type == JTokenType.Array)
                                    {
                                        // this is where we find multiple values if the range > 1d.
                                        if (value.First.Type == JTokenType.Float)
                                        {
                                            int index = 0;
                                            foreach (JToken number in (JArray)value)
                                            {
                                                if (number.Type == JTokenType.Float || number.Type == JTokenType.Integer)
                                                {
                                                    list[index].Volume = (decimal)number;
                                                }
                                                index++;
                                            }
                                        }
                                    }
                                    if (quote.TryGetValue("close", StringComparison.Ordinal, out value) && value.Type == JTokenType.Array)
                                    {
                                        // this is where we find multiple values if the range > 1d.
                                        if (value.First.Type == JTokenType.Float)
                                        {
                                            int index = 0;
                                            foreach (JToken number in (JArray)value)
                                            {
                                                if (number.Type == JTokenType.Float || number.Type == JTokenType.Integer)
                                                {
                                                    list[index].Close = (decimal)number;
                                                }
                                                index++;
                                            }
                                        }
                                    }
                                    if (quote.TryGetValue("high", StringComparison.Ordinal, out value) && value.Type == JTokenType.Array)
                                    {
                                        // this is where we find multiple values if the range > 1d.
                                        if (value.First.Type == JTokenType.Float)
                                        {
                                            int index = 0;
                                            foreach (JToken number in (JArray)value)
                                            {
                                                if (number.Type == JTokenType.Float || number.Type == JTokenType.Integer)
                                                {
                                                    list[index].High = (decimal)number;
                                                }
                                                index++;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return list;
        }

        protected override async Task<bool> DownloadThrottledQuoteHistoryAsync(StockQuoteHistory history)
        {
            string range = "max";
            var entry = history.History.LastOrDefault();
            if (entry != null)
            {
                var span = entry.Date - DateTime.UtcNow;
                for (int i = 0, n = validRangeSpans.Count(); i < n; i++)
                {
                    if (span > validRangeSpans[i])
                    {
                        range = validRanges[i];
                    }
                }
            }

            var list = await this.DownloadChart(history.Symbol, range);
            foreach (var quote in list)
            {
                history.AddQuote(quote);
            }

            if (range == "max")
            {
                // this was a summary, now try and get the last year complete daily values.
                list = await this.DownloadChart(history.Symbol, "1y");
                foreach (var quote in list)
                {
                    history.AddQuote(quote);
                }
            }

            history.Complete = true;
            return true;
        }
    }
}

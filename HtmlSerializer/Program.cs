

// See https://aka.ms/new-console-template for more information

using HtmlSerializer;
using System.Text.RegularExpressions;
var html = await Load("https://books.toscrape.com/"); 

var root = new HtmlElement();
var currentElement = root;

var htmlLines = new Regex("<([^>]*)>")
    .Split(html)
    .Where(s => !string.IsNullOrWhiteSpace(s));

var helper = HtmlHelper.Instance;

foreach (var item in htmlLines)
{
    var currentItem = item.Trim();

    var firstWord = Regex.Split(currentItem, @"\s+")[0];

    if (firstWord == "/html")
    {
        break;
    }
    if (firstWord.StartsWith("/"))
    {
        if (currentElement.Parent != null)
        {
            currentElement = currentElement.Parent;
        }

        continue;
    }

    if (helper.HtmlTags.Contains(firstWord))
    {
        var newElement = new HtmlElement();

        newElement.Name = firstWord;
        newElement.Parent = currentElement;

        currentElement.Children.Add(newElement);

        var attributesText = currentItem.Substring(firstWord.Length).Trim();

        var matches =
            new Regex("([^\\s=]+)=\"(.*?)\"")
            .Matches(attributesText);

        foreach (Match match in matches)
        {
            var attributeName = match.Groups[1].Value;
            var attributeValue = match.Groups[2].Value;

            newElement.Attributes.Add(
                $"{attributeName}=\"{attributeValue}\""
            );

            if (attributeName == "id")
            {
                newElement.Id = attributeValue;
            }

            if (attributeName == "class")
            {
                var classes = attributeValue.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries
                );

                newElement.Classes.AddRange(classes);
            }
        }

        var isSelfClosing =
            currentItem.EndsWith("/") ||
            helper.HtmlVoidTags.Contains(firstWord);

        if (!isSelfClosing)
        {
            currentElement = newElement;
        }
    }
    else
    {
        currentElement.InnerHtml += item;
    }
}

var selector = Selector.Parse("article");
var result = root.FindElements(selector);

Console.WriteLine(result.Count());


async Task<string> Load(string url)
{
    HttpClient client = new HttpClient();
    var response = await client.GetAsync(url);
    var html = await response.Content.ReadAsStringAsync();
    return html;
}




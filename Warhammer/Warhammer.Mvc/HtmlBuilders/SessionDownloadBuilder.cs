//using EvoPdf.HtmlToPdf;

//namespace Warhammer.Mvc.HtmlBuilders
//{
//	public class SessionDownloadBuilder
//	{
//		private IViewModelFactory _viewModelFactory;
//		public IViewModelFactory ViewModelFactory { get { return _viewModelFactory; } }

//		public SessionDownloadBuilder(IViewModelFactory viewModelFactory)
//		{
//			_viewModelFactory = viewModelFactory;
//		}

//		/// <summary>
//		/// Convert the specified HTML string to a PDF document and send the 
//		/// document to the browser
//		/// </summary>
//		private byte[] ConvertHTMLStringToPDF(string htmlString, string absoluteUri, string headerHtml, string footerHtml)
//		{
//			// Create the PDF converter. Optionally the HTML viewer width can be specified as parameter
//			// The default HTML viewer width is 1024 pixels.
//			PdfConverter pdfConverter = new PdfConverter();

//			// set the license key
//			pdfConverter.LicenseKey = "MxgBEwAAEwcFAxMBHQMTAAIdAgEdCgoKCg==";

//			pdfConverter.PdfDocumentOptions.PdfPageSize = PdfPageSize.A4;
//			pdfConverter.PdfDocumentOptions.PdfCompressionLevel = PdfCompressionLevel.Normal;
//			pdfConverter.PdfDocumentOptions.PdfPageOrientation = PdfPageOrientation.Portrait;
//			pdfConverter.PdfBookmarkOptions.HtmlElementSelectors = new string[] { "H1", "H2" };

//			AddHeader(pdfConverter, headerHtml, absoluteUri);
//			AddFooter(pdfConverter, footerHtml, absoluteUri);

//			return pdfConverter.GetPdfBytesFromHtmlString(htmlString, absoluteUri);
//		}

//		private void AddHeader(PdfConverter pdfConverter, string headerHtml, string absoluteUri)
//		{
//			pdfConverter.PdfDocumentOptions.ShowHeader = true;
//			pdfConverter.PdfHeaderOptions.HeaderHeight = 50;
//			pdfConverter.PdfHeaderOptions.DrawHeaderLine = false;
//			pdfConverter.PdfHeaderOptions.HtmlToPdfArea = new HtmlToPdfArea(headerHtml, absoluteUri);
//			pdfConverter.PdfHeaderOptions.HtmlToPdfArea.FitHeight = true;
//			pdfConverter.PdfHeaderOptions.HtmlToPdfArea.EmbedFonts = true;
//			pdfConverter.PdfHeaderOptions.ShowOnFirstPage = true;
//		}

//		private void AddFooter(PdfConverter pdfConverter, string footerHtml, string absoluteUri)
//		{
//			pdfConverter.PdfDocumentOptions.ShowFooter = true;
//			pdfConverter.PdfFooterOptions.FooterHeight = 50;
//			pdfConverter.PdfFooterOptions.DrawFooterLine = false;
//			pdfConverter.PdfFooterOptions.HtmlToPdfArea = new HtmlToPdfArea(footerHtml, absoluteUri);
//			pdfConverter.PdfFooterOptions.HtmlToPdfArea.FitHeight = true;
//			pdfConverter.PdfFooterOptions.HtmlToPdfArea.EmbedFonts = true;
//			pdfConverter.PdfFooterOptions.ShowOnFirstPage = true;
//		}

//		public byte[] GeneratePdfDownloadForSession(Guid userId, int sessionId, string cssPath)
//		{
//			SessionViewModel session = ViewModelFactory.GetSessionForCurrentUser(userId, sessionId);
//			List<PostViewModel> posts = ViewModelFactory.GetPostsForCurrentUserInSessionSinceLast(userId, sessionId, 0);

//			if (session != null)
//			{
//				CampaignViewModel campaign = ViewModelFactory.GetCampaign(session.CampaignId);
//				if (campaign != null)
//				{
//					PlayerViewModel gm = ViewModelFactory.GetPlayer(campaign.GmId);
//					posts = (from p in posts
//							 orderby p.ID ascending
//							 select p).ToList();

//					PostBuilder postBuilder = new PostBuilder();

//					string absoluteUri = "http://roleplay.sendingofeight.co.uk";
//					string headerHtml = string.Format("<html><body><span style=\"display:block; text-align:center;\">{0}</span></body></html>", session.Title);
//					string footerHtml = string.Format("<html><body><span style=\"display:block; text-align:center;\">{0}</span></body></html>", absoluteUri);

//					StringBuilder html = new StringBuilder();

//					html.Append("<html><head>");
//					html.Append("<link rel=\"Stylesheet\" type=\"text/css\" href=\"");
//					html.Append(cssPath);
//					html.Append("\" />");
//					html.Append("</head><body>");

//					html.Append("<div class=\"SessionContainer\">");

//					html.Append("<div class=\"SessionHeaderSection\"><h1>");
//					html.Append(session.Title);
//					html.Append("</h1><h2>GM: ");
//					if (gm != null)
//					{
//						html.Append(gm.Name);
//					}
//					else
//					{
//						html.Append("Unknown");
//					}
//					html.Append("</h2></div>");

//					html.Append("<div class=\"SessionPostsSection\">");
//					foreach (PostViewModel post in posts)
//					{
//						html.Append(postBuilder.GetHtmlForPost(post));
//					}
//					html.Append("</div>");
//					html.Append("</div></body></html>");

//					//TextWriter tw = new StreamWriter("C:\\users\\chris\\desktop\\test.html");
//					//tw.Write(html.ToString());
//					//tw.Close();
//					//tw.Dispose();

//					return ConvertHTMLStringToPDF(html.ToString(), absoluteUri, headerHtml, footerHtml);
//				}
//			}

//			return new byte[0];
//		}


//	}
//}
using System;
using System.Diagnostics;
using System.Text;

namespace Squid
{
//================================================================================================//
// HTML Helper Class.                                                                             //
//                                                                                                //
/// <summary>
/// <para>
///      This class provides miscellaneous resources to help with working with HTML.
/// </para>
/// </summary>
///
public
static
class HtmlHelper
{
   //---------------------------------------------------------------------------------------------//
   // Escape String For HTML.                                                                     //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This function escapes a specified string so that it can be expressed in HTML, that
   /// is, it will return the HTML required to display the specified string.
   /// </para>
   /// </summary>
   ///
   /// <param name="s">
   ///      The string to be converted.  This may contain any characters, uncluding "special
   /// HTML" characters like "&lt;".  This may be NULL or empty.
   /// </param>
   ///
   /// <returns>
   ///      The specified string with its "special HTML" characters properly escaped so the
   /// original string can be displayed in HTML.  This may not be NULL but may be empty.
   /// </returns>
   ///
   public
   static
   String
   EscapeStringForHtml(String s)
   {
      return System.Security.SecurityElement.Escape(s??String.Empty);
   }
   //---------------------------------------------------------------------------------------------//
   // Create Link HTML.                                                                           //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This function returns the HTML for a link to the specified URL and displaying the
   /// specified text.
   /// </para>
   /// </summary>
   ///
   public
   static
   String
   CreateLinkHtml(String url,
                  String linkText)
   {
      Debug.Assert(!String.IsNullOrEmpty(url     ));
      Debug.Assert(!String.IsNullOrEmpty(linkText));

      StringBuilder stringBuilder = new StringBuilder();

      stringBuilder.Append("<a href = \""                          );
      stringBuilder.Append(url                                     );
      stringBuilder.Append("\">"                                   );
      stringBuilder.Append(HtmlHelper.EscapeStringForHtml(linkText));
      stringBuilder.Append("</a>"                                  );

      return stringBuilder.ToString();
   }
   //---------------------------------------------------------------------------------------------//
   // Create URL.                                                                                 //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This function creates a URL by concatenating together a base URL string and 0 or more
   /// child path strings.
   /// </para>
   /// </summary>
   ///
   /// <param name="baseUrl">
   ///      The initial portion, base portion, of the URL.  The URL returned will begin with this
   /// string.  This may not be NULL or empty.  This may end with a path seperator character
   /// ("/") but this is not required.
   /// </param>
   ///
   /// <param name="childPaths">
   ///      The list of child paths to be appened onto the base URL.  These paths will be
   /// concatenated onto the URL in the order that they are speciried in this list.  The list may
   /// not be NULL but may be empty.  The list may not contain entries that are NULL or empty.
   /// The entries in the list may end with a path seperator character ("/"), but this is not
   /// required.  Typically each entry in the list will specify a single directory or file of the
   /// desired path; however, a single entry could contain multiple path elements if they are
   /// sperated by path seperator characters.  i.e. a single entry could contain
   /// "DataFolder/File.Dat".
   /// </param>
   ///
   /// <returns>
   ///      The resulting URL. This may not be NULL or empty.  This url may, or may not, end with
   /// a path seperator character.  This is controlled by the last element concatenated onto the
   /// URL.
   /// </returns>
   ///
   public
   static
   String
   CreateUrl(String          baseUrl,
             params String[] childPaths)
   {
      Debug.Assert(!String.IsNullOrEmpty(baseUrl));

      bool          isSeperatorNeeded = baseUrl[baseUrl.Length - 1] != '/';
      int           childPathCount    = childPaths.Length;
      StringBuilder stringBuilder     = new StringBuilder(baseUrl);

      for (int i = 0; i < childPathCount; ++i)
      {
         String childPath = childPaths[i];

         Debug.Assert(!String.IsNullOrEmpty(childPath));

         if (isSeperatorNeeded)
            stringBuilder.Append('/');
         stringBuilder.Append(childPath);
         isSeperatorNeeded = childPath[childPath.Length - 1] != '/';
      }
      return stringBuilder.ToString();
   }
   //---------------------------------------------------------------------------------------------//
}
//================================================================================================//
}


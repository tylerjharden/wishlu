//================================================================================================//
// JSON Helper Class.                                                                             //
//                                                                                                //
// SYNOPSIS                                                                                       //
//      This unit implements the JSON helper class.                                               //
//                                                                                                //
// DESCRIPTION                                                                                    //
//      This unit implements the JSON helper class.  This class provides miscellaneous resources  //
// to help with working with JSON-format data.                                                    //
//                                                                                                //
// Copyright (c) 2013 WishLu, Inc.  All rights reserved.                                          //
//------------------------------------------------------------------------------------------------//

using System;

namespace Squid
{
//================================================================================================//
// JSON Helper Class.                                                                             //
//                                                                                                //
/// <summary>
/// <para>
///      This class provides miscellaneous resources to help with working with JSON.
/// </para>
/// </summary>
///
public
static
class JsonHelper
{
   //---------------------------------------------------------------------------------------------//
   // Convert Date And Time to JSON.                                                              //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This function converts the specified date and time value to a string that can
   /// represent the date and time in JSON.
   /// </para>
   /// </summary>
   ///
   /// <remarks>
   /// <para>
   ///      The procedure returns a date and time value in the .net "round trip" format, an ISO
   /// 8601 format.  This has the format yyyy-MM-ddThh:mm:ss.sssssss-zz:zz.
   /// </para>
   /// </remarks>
   ///
   /// <param name="dateTime">
   ///      The date and time to be converted.  This may be NULL.
   /// </param>
   ///
   /// <returns>
   ///      The string that represents the specified date and time in a JSON-compatible format.
   /// This will be empty if the specified date and time value was NULL.  This may not be NULL.
   /// </returns>
   ///
   public
   static
   String
   DateAndTimeToJson(DateTime? dateTime)
   {
      if (!dateTime.HasValue)
         return String.Empty;
      return dateTime.Value.ToString("O");
   }
   //---------------------------------------------------------------------------------------------//
   // Convert Date to JSON.                                                                       //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This function converts the specified date value to a string that can represent the
   /// date in JSON.
   /// </para>
   /// </summary>
   ///
   /// <remarks>
   /// <para>
   ///      The procedure returns a date and time value the MM/dd/yyyy format.
   /// </para>
   /// </remarks>
   ///
   /// <param name="dateTime">
   ///      The date to be converted.  This may be NULL.
   /// </param>
   ///
   /// <returns>
   ///      The string that represents the specified date in a JSON-compatible format. This will
   /// be empty if the specified date value was NULL.  This may not be NULL.
   /// </returns>
   ///
   public
   static
   String
   DateToJson(DateTime? dateTime)
   {
      if (!dateTime.HasValue)
         return String.Empty;
      return dateTime.Value.ToString("MM/dd/yyyy");
   }
   //---------------------------------------------------------------------------------------------//
   // Parse Date And Time.                                                                        //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This function parses a data and time value from the specified JSON data string.
   /// </para>
   /// </summary>
   ///
   /// <remarks>
   /// <para>
   ///      The procedure expectes a date and time value in the .net "round trip" format, an ISO
   /// 8601 format.  This has the format yyyy-MM-ddThh:mm:ss.sssssss-zz:zz.
   /// </para>
   /// </remarks>
   ///
   /// <param name="value">
   ///      The string that specifies the value to be parsed.  This may be NULL or empty.
   /// </param>
   ///
   /// <returns>
   ///      The date and time value parsed from the specified string.  This may be NULL if the
   /// specified string is NULL or empty.
   /// </returns>
   ///
   /// <exception cref="FormatException">
   ///      The specified value did represent a data and time in the supported format.
   /// </exception>
   ///
   public
   static
   DateTimeOffset
   ParseDateAndTime(String value)
   {
      if (String.IsNullOrEmpty(value))
         return DateTimeOffset.MinValue;

      return DateTimeOffset.Parse(value,null,System.Globalization.DateTimeStyles.RoundtripKind);
   }
   //---------------------------------------------------------------------------------------------//
   // Parse Date.                                                                                 //
   //                                                                                             //
   /// <summary>
   /// <para>
   ///      This function parses a data value from the specified JSON data string.
   /// </para>
   /// </summary>
   ///
   /// <remarks>
   /// <para>
   ///      The procedure expectes a date value in the MM/dd/yyyy format.
   /// </para>
   /// </remarks>
   /// <param name="value">
   ///      The string that specifies the value to be parsed.  This may be NULL or empty.
   /// </param>
   ///
   /// <returns>
   ///      The date value parsed from the specified string.  This may be NULL if the specified
   /// string is NULL or empty.
   /// </returns>
   ///
   /// <exception cref="FormatException">
   ///      The specified value did represent a data in the supported format.
   /// </exception>
   ///
   public
   static
   DateTimeOffset
   ParseDate(String value)
   {
      if (String.IsNullOrEmpty(value))
         return DateTimeOffset.MinValue;

      return DateTimeOffset.ParseExact(value,"MM/dd/yyyy",null,System.Globalization.DateTimeStyles.None);
   }
   //---------------------------------------------------------------------------------------------//
}
//================================================================================================//
}


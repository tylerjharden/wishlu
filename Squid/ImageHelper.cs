//================================================================================================//
// Image Helper Class.                                                                            //
//                                                                                                //
// SYNOPSIS                                                                                       //
//      This unit implements the Image helper class.                                              //
//                                                                                                //
// DESCRIPTION                                                                                    //
//      This unit implements the Image helper class.  This class provides miscellaneous resources //
// to help with working with Images and image files.                                              //
//                                                                                                //
// Copyright (c) 2013 WishLu, Inc.  All rights reserved.                                          //
//------------------------------------------------------------------------------------------------//

using System;
using System.Diagnostics;

namespace Squid
{
    //================================================================================================//
    // Image Helper Class.                                                                            //
    //                                                                                                //
    /// <summary>
    /// <para>
    ///      This class provides miscellaneous resources to help with working with Images and image
    /// fules.
    /// </para>
    /// </summary>
    ///
    public
    static
    class ImageHelper
    {
        //---------------------------------------------------------------------------------------------//
        // Does Data Start With?                                                                       //
        //                                                                                             //
        /// <summary>
        /// <para>
        ///      This function returns a boolean value that indicates if the specified byte array to
        /// be checked starts with the specifed array of bytes to check for.
        /// </para>
        /// </summary>
        ///
        /// <param name="bytesToCheck">
        ///      The array of bytes to be checked, that is, to the bytes to be checked to see if they
        /// begin with the bytes to check for.  This may not be NULL and must be at least as long as
        /// the bytesToCheckFor array.
        /// </param>
        ///
        /// <param name="bytesToCheckFor">
        ///      The array of bytes to check for.  This may not be NULL and must not be longer than
        /// the array of bytes to be checked.
        /// </param>
        ///
        /// <returns>
        ///      Does the specified array of bytes to checkbegin with the specified array of bytes to
        /// check for.
        /// </returns>
        ///
        private
        static
        bool
        DoesDataStartWith(Byte[] bytesToCheck,
                          Byte[] bytesToCheckFor)
        {
            Debug.Assert(bytesToCheck != null);
            Debug.Assert(bytesToCheckFor != null);
            Debug.Assert(bytesToCheck.Length >= bytesToCheckFor.Length);

            int byteCount = bytesToCheckFor.Length;

            for (int i = 0; i < byteCount; ++i)
            {
                if (bytesToCheck[i] != bytesToCheckFor[i])
                    return false;
            }
            return true;
        }
        //---------------------------------------------------------------------------------------------//
        // Get File's Extension.                                                                       //
        //                                                                                             //
        /// <summary>
        /// <para>
        ///      This function returns the file extension that should be used for the specified image
        /// file data.
        /// </para>
        /// </summary>
        ///
        /// <param name="imageData">
        ///      The image data to be tested to determine the image data format and thus the file name
        /// extension.  This may be NULL or empty, but that will trigger an
        /// ImageFormatNotSupportedException;
        /// </param>
        ///
        /// <exception cref="ImageFormatNotSupportedException">
        ///      The image format could not be detected or is not spported.
        /// </exception>
        ///
        /// <returns>
        ///      The file name extension to be used to store a image file with the specified data.
        /// This may not be NULL or empty.  This will not contain the leading "." at the start of the
        /// extension.
        /// </returns>
        ///
        public
        static
        String
        GetFileExtension(Byte[] imageData)
        {
            if (imageData == null || imageData.Length < 4)
            {
                String message = ("Service.Image.ImageFormatNotSupported");

                throw new ImageFormatNotSupportedException(message, "Data may have been truncated.");
            }
            if (DoesDataStartWith(imageData, new Byte[] { (Byte)'G', (Byte)'I', (Byte)'F' }))
                return "gif";
            if (DoesDataStartWith(imageData, new Byte[] { 137, 80, 78, 71 }))
                return "png";
            if (DoesDataStartWith(imageData, new Byte[] { 73, 73, 42 }))
                return "tiff";
            if (DoesDataStartWith(imageData, new Byte[] { 77, 77, 42 }))
                return "tiff";
            if (DoesDataStartWith(imageData, new Byte[] { 255, 216, 255, 224 }))
                return "jpeg";
            if (DoesDataStartWith(imageData, new Byte[] { 255, 216, 255, 225 }))
                return "jpeg";

            {
                String message = ("Service.Image.ImageFormatNotSupported");

                throw new ImageFormatNotSupportedException(message);
            }
        }
        //---------------------------------------------------------------------------------------------//
    }
    //================================================================================================//
}
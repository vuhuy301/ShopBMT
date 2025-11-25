using Microsoft.AspNetCore.Mvc;
using backend_shopcaulong.Services;

namespace backend_shopcaulong.Controllers
{
    [Route("api/test-upload")]
    [ApiController]
    public class UploadsController : ControllerBase
    {
        private readonly IUploadService _uploadService;

        public UploadsController(IUploadService uploadService)
        {
            _uploadService = uploadService;
        }

        /// <summary>
        /// 1. TEST UPLOAD ẢNH CHÍNH SẢN PHẨM (nhiều ảnh - gallery)
        /// POST /api/test-upload/main-images
        /// form-data key: files → chọn nhiều ảnh
        /// </summary>
        [HttpPost("main-images")]
        public async Task<IActionResult> TestUploadMainImages(IFormFileCollection files)
        {
            if (files == null || files.Count == 0)
                return BadRequest(new { message = "Vui lòng chọn ít nhất 1 file!" });

            try
            {
                var urls = await _uploadService.UploadProductImagesAsync(files);
                return Ok(new
                {
                    message = $"Upload thành công {urls.Count} ảnh chính!",
                    count = urls.Count,
                    urls = urls
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi upload ảnh chính", error = ex.Message });
            }
        }

        /// <summary>
        /// 2. TEST UPLOAD ẢNH CHI TIẾT (dùng cho ProductDetail.ImageFile)
        /// POST /api/test-upload/detail-image
        /// form-data key: file → chọn 1 ảnh
        /// </summary>
        [HttpPost("detail-image")]
        public async Task<IActionResult> TestUploadDetailImage(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Vui lòng chọn 1 file ảnh!" });

            try
            {
                var url = await _uploadService.UploadDetailImageAsync(file);
                return Ok(new
                {
                    message = "Upload ảnh chi tiết thành công!",
                    url = url
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi upload ảnh chi tiết", error = ex.Message });
            }
        }

        /// <summary>
        /// 3. TEST UPLOAD ẢNH RIÊNG CHO BIẾN THỂ (Variant)
        /// POST /api/test-upload/variant-image
        /// form-data key: file → chọn 1 ảnh (ví dụ: ảnh vợt màu đỏ)
        /// </summary>
        [HttpPost("variant-image")]
        public async Task<IActionResult> TestUploadVariantImage(IFormFile? file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Vui lòng chọn 1 file ảnh cho biến thể!" });

            try
            {
                var url = await _uploadService.UploadVariantImageAsync(file);
                return Ok(new
                {
                    message = "Upload ảnh biến thể (variant) thành công!",
                    url = url,
                    tip = "Dùng URL này cho MainImageUrl của Variant (màu đỏ, xanh...)"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi upload ảnh biến thể", error = ex.Message });
            }
        }

        /// <summary>
        /// Xem tất cả thư mục ảnh đã upload – để kiểm tra bằng mắt
        /// GET /api/test-upload/sample
        /// </summary>
        [HttpGet("sample")]
        public IActionResult GetSampleLinks()
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";

            return Ok(new
            {
                message = "Mở các link dưới đây trong trình duyệt để xem ảnh đã upload",
                ảnh_chính_sản_phẩm = $"{baseUrl}/images/products/",
                ảnh_chi_tiết_mô_tả = $"{baseUrl}/images/products/details/",
                ảnh_riêng_theo_màu_variant = $"{baseUrl}/images/products/variants/",
                tip = "Sau khi upload → mở link → thấy ảnh là thành công!"
            });
        }
    }
}
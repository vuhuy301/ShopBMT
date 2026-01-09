// utils/productValidation.js

/**
 * Validate product trước khi gửi lên server
 * Dùng cho cả Add và Update
 */
export const validateProduct = (product, { isAdd = true } = {}) => {
  const errors = {};

  // Tên sản phẩm
  if (!product.name?.trim()) {
    errors.name = "Tên sản phẩm là bắt buộc";
  }

  // Mô tả
  if (!product.description?.trim()) {
    errors.description = "Mô tả là bắt buộc";
  }

  // Giá
  if (!product.price || Number(product.price) <= 0) {
    errors.price = "Giá phải lớn hơn 0";
  }

  // Giá giảm (nếu có)
  if (
    product.discountPrice &&
    Number(product.discountPrice) > Number(product.price)
  ) {
    errors.discountPrice = "Giá giảm phải nhỏ hơn hoặc bằng giá gốc";
  }

  // Brand + Category
  if (!product.brandId) errors.brandId = "Chọn thương hiệu";
  if (!product.categoryId) errors.categoryId = "Chọn danh mục";

  // Ảnh chính
  if (isAdd && (!product.mainImages || product.mainImages.length === 0)) {
    errors.mainImages = "Phải chọn ít nhất 1 ảnh chính";
  }

  // Chi tiết mô tả
  if (product.details?.length) {
    product.details.forEach((d, i) => {
      if (!d.text?.trim()) {
        errors[`details[${i}]`] = "Chi tiết mô tả không được để trống";
      }
    });
  }

  // Biến thể màu
  if (product.colorVariants?.length) {
    product.colorVariants.forEach((cv, i) => {
      if (!cv.color?.trim()) {
        errors[`colorVariants[${i}]`] = "Tên màu không được để trống";
      }

      cv.sizes.forEach((s, j) => {
        if (!s.size?.trim()) {
          errors[`colorVariants[${i}].sizes[${j}].size`] = "Size không được để trống";
        }
        if (s.stock == null || s.stock < 0) {
          errors[`colorVariants[${i}].sizes[${j}].stock`] = "Stock phải >= 0";
        }
      });
    });
  }

  return errors;
};

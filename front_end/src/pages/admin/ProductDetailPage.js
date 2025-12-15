import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getProductById } from "../../services/productService";
import styles from "./ProductDetailPage.module.css";

const IMAGE_BASE = process.env.REACT_APP_IMAGE_BASE_URL;

const ProductDetailPage = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const [product, setProduct] = useState(null);

    useEffect(() => {
        const loadProduct = async () => {
            const data = await getProductById(id);
            setProduct(data);
        };
        loadProduct();
    }, [id]);

    if (!product) return <p>Đang tải...</p>;

    return (
        <div className={styles.container}>
            <h2>Chi tiết sản phẩm</h2>

            {/* THÔNG TIN CHUNG */}
            <div className={styles.section}>
                <h3>Thông tin chung</h3>
                <p><b>Tên:</b> {product.name}</p>
                <p><b>Mô tả:</b> {product.description}</p>
                <p><b>Giá:</b> {product.price}</p>
                <p><b>Giá giảm:</b> {product.discountPrice}</p>
                <p><b>Nổi bật:</b> {product.isFeatured ? "Có" : "Không"}</p>
                <p><b>Danh mục:</b> {product.categoryName}</p>
                <p><b>Thương hiệu:</b> {product.brandName}</p>
            </div>

            {/* ẢNH SẢN PHẨM */}
            <div className={styles.section}>
                <h3>Ảnh sản phẩm</h3>
                <div className={styles.imageList}>
                    {product.images?.length > 0 ? (
                        product.images.map((img, i) => (
                            <img
                                key={i}
                                src={IMAGE_BASE + img.imageUrl}
                                alt=""
                                className={styles.image}
                            />
                        ))
                    ) : (
                        <p>Không có ảnh</p>
                    )}
                </div>
            </div>

            {/* CHI TIẾT MÔ TẢ */}
            <div className={styles.section}>
                <h3>Chi tiết mô tả</h3>
                {product.details?.length > 0 ? (
                    product.details.map((d, i) => (
                        <div key={i} className={styles.detailItem}>
                            <p><b>Sort:</b> {d.sortOrder}</p>
                            <p>{d.text}</p>
                            {d.imageUrl && <img src={IMAGE_BASE + d.imageUrl} alt="" className={styles.detailImage} />}
                        </div>
                    ))
                ) : (
                    <p>Không có mô tả chi tiết</p>
                )}
            </div>

            {/* BIẾN THỂ MÀU */}
            {/* Ảnh biến thể màu */}
            <div className={styles.section}>
                <h3>Biến thể màu</h3>

                {product.colorVariants?.map((cv, i) => (
                    <div key={i} className={styles.colorBox}>
                        <p><b>Màu:</b> {cv.color}</p>

                        {/* Ảnh biến thể */}
                        <div className={styles.imageList}>
                            {cv.imageUrls?.length > 0 ? (
                                cv.imageUrls.map((url, j) => (
                                    <img
                                        key={j}
                                        src={IMAGE_BASE + url}
                                        alt=""
                                        className={styles.variantImg}
                                    />
                                ))
                            ) : (
                                <p>Không có ảnh biến thể</p>
                            )}
                        </div>

                        {/* Sizes */}
                        <h4>Sizes</h4>
                        {cv.sizes?.map((s, j) => (
                            <p key={j}>
                                Size: {s.size} — Tồn kho: {s.stock}
                            </p>
                        ))}
                    </div>
                ))}
            </div>


            {/* NÚT EDIT */}
            <button
                className={styles.btnEdit}
                onClick={() => navigate(`/admin/product/${id}/edit`)}
            >
                Cập nhật sản phẩm
            </button>
        </div>
    );
};

export default ProductDetailPage;

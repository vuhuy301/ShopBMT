import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import styles from "./ProductAdmin.module.css";
import ProductCard from "./ProductCard";
import { getProducts } from "../../services/productService";

const ProductAdmin = () => {
    const [products, setProducts] = useState([]);
    const [page, setPage] = useState(1);
    const [pageSize] = useState(10);

    const navigate = useNavigate();
    // Filter
    const [search, setSearch] = useState("");
    const [categoryId, setCategoryId] = useState("");

    const [totalPages, setTotalPages] = useState(1);

    const loadData = async () => {
        const data = await getProducts({
            categoryId,
            page,
            pageSize,
            search,
        });

        setProducts(data.items ?? []);
        setTotalPages(data.totalPages ?? 1);
    };

    useEffect(() => {
        loadData();
    }, [page, search, categoryId]);

    return (
        <div className={styles.container}>
            <h2 className={styles.title}>Quản lý sản phẩm</h2>
            <div className="d-flex justify-content-between align-items-center mb-3">

                {/* Nút thêm sản phẩm */}
                <button
                    className="btn btn-success"
                    onClick={() => navigate("/admin/add-product")}
                >
                    + Thêm sản phẩm
                </button>

                {/* Nhóm filter */}
                <div className="d-flex gap-2">
                    <input
                        type="text"
                        className="form-control"
                        placeholder="Tìm theo tên..."
                        value={search}
                        onChange={(e) => {
                            setPage(1);
                            setSearch(e.target.value);
                        }}
                        style={{ width: "200px" }}
                    />

                    <select
                        className="form-select"
                        value={categoryId}
                        onChange={(e) => {
                            setPage(1);
                            setCategoryId(e.target.value);
                        }}
                        style={{ width: "180px" }}
                    >
                        <option value="">Danh mục</option>
                        <option value="2">Giày cầu lông</option>
                        <option value="1">Vợt cầu lông</option>
                    </select>
                </div>

            </div>



            {/* LIST */}
            <div className={styles.list}>
                {products.length === 0 ? (
                    <p>Không có sản phẩm</p>
                ) : (
                    products.map((p) => <ProductCard key={p.id} product={p} />)
                )}
            </div>

            {/* PAGINATION */}
            <div className={styles.pagination}>
                <button disabled={page === 1} onClick={() => setPage(page - 1)}>
                    «
                </button>

                <span>
                    Trang {page}/{totalPages}
                </span>

                <button
                    disabled={page >= totalPages}
                    onClick={() => setPage(page + 1)}
                >
                    »
                </button>
            </div>
        </div>
    );
};

export default ProductAdmin;

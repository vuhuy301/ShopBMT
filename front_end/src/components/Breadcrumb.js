import { Link } from "react-router-dom";
import styles from "./Breadcrumb.module.css";

const Breadcrumb = ({ items }) => {
  return (
    <nav className={styles.breadcrumb}>
      {items.map((item, index) => {
        const isLast = index === items.length - 1;

        return (
          <span key={index}>
            {index > 0 && " / "}
            {isLast || !item.path ? (
              <span className={styles.current}>{item.label}</span>
            ) : (
              <Link to={item.path}>{item.label}</Link>
            )}
          </span>
        );
      })}
    </nav>
  );
};

export default Breadcrumb;

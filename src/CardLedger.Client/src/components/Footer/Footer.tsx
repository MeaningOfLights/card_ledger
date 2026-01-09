import Navbar from "../Navbar/Navbar";
import styles from "./Footer.module.scss";

const Footer = () => {
  return (
    <footer className={styles.footer}>
      <p className={styles.title}>Copyright Jeremy Thompson @ {new Date().getFullYear()}</p>
      <Navbar />
    </footer>
  );
};

export default Footer;

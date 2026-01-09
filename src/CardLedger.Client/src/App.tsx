import { Routes, Route } from "react-router-dom";
import Header from "./components/Header/Header";
import Footer from "./components/Footer/Footer";
import About from "./pages/About/About";
import CardLedger from "./pages/CardLedger/CardLedger";
import NotFound from "./pages/NotFound/NotFound";
import "./styles/main.scss";

function App() {
  return (
    <>
      <Header />
      <Routes>
        <Route path="/" element={<CardLedger />} />
        <Route path="/about" element={<About />} />
        <Route path="*" element={<NotFound />} />
      </Routes>
      <Footer />
    </>
  );
}

export default App;

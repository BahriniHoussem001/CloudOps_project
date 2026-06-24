import { useState } from "react";
import Dashboard from "./pages/Dashboard";
import Users from "./pages/Users";
import Services from "./pages/Services";
import Requests from "./pages/Requests";
import Notifications from "./pages/Notifications";
import "./App.css";

function App() {
  const [activePage, setActivePage] = useState("dashboard");

  const renderPage = () => {
    switch (activePage) {
      case "users":
        return <Users />;
      case "services":
        return <Services />;
      case "requests":
        return <Requests />;
      case "notifications":
        return <Notifications />;
      default:
        return <Dashboard />;
    }
  };

  return (
    <div className="app">
      <aside className="sidebar">
        <h2>CloudOps</h2>

        <button onClick={() => setActivePage("dashboard")}>Dashboard</button>
        <button onClick={() => setActivePage("users")}>Users</button>
        <button onClick={() => setActivePage("services")}>Services</button>
        <button onClick={() => setActivePage("requests")}>Requests</button>
        <button onClick={() => setActivePage("notifications")}>
          Notifications
        </button>
      </aside>

      <main className="content">{renderPage()}</main>
    </div>
  );
}

export default App;
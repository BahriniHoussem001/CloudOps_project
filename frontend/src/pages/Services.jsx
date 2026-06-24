import { useEffect, useState } from "react";
import apiClient from "../api/apiClient";

function Services() {
  const [services, setServices] = useState([]);
  const [error, setError] = useState("");

  useEffect(() => {
    apiClient
      .get("/services")
      .then((response) => {
        setServices(response.data.data);
      })
      .catch(() => {
        setError("Failed to load services");
      });
  }, []);

  return (
    <div>
      <h1>Services</h1>
      <p>Available services loaded from the backend.</p>

      {error && <p className="error">{error}</p>}

      <div className="card">
        {services.length === 0 ? (
          <p>No services found.</p>
        ) : (
          <table>
            <thead>
              <tr>
                <th>Name</th>
                <th>Category</th>
                <th>Description</th>
              </tr>
            </thead>

            <tbody>
              {services.map((service) => (
                <tr key={service.id}>
                  <td>{service.name}</td>
                  <td>{service.category}</td>
                  <td>{service.description}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}

export default Services;
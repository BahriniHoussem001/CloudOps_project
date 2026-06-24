import { useEffect, useState } from "react";
import apiClient from "../api/apiClient";

function Requests() {
  const [requests, setRequests] = useState([]);
  const [users, setUsers] = useState([]);
  const [services, setServices] = useState([]);
  const [error, setError] = useState("");
  const [successMessage, setSuccessMessage] = useState("");

  const [formData, setFormData] = useState({
    clientId: "",
    serviceId: "",
    title: "",
    description: "",
  });

  const loadData = async () => {
    try {
      const [requestsResponse, usersResponse, servicesResponse] =
        await Promise.all([
          apiClient.get("/requests"),
          apiClient.get("/users"),
          apiClient.get("/services"),
        ]);

      setRequests(requestsResponse.data.data);
      setUsers(usersResponse.data.data);
      setServices(servicesResponse.data.data);
    } catch {
      setError("Failed to load requests data");
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const handleChange = (event) => {
    const { name, value } = event.target;

    setFormData((previousData) => ({
      ...previousData,
      [name]: value,
    }));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError("");
    setSuccessMessage("");

    try {
      await apiClient.post("/requests", formData);

      setSuccessMessage("Service request created successfully");

      setFormData({
        clientId: "",
        serviceId: "",
        title: "",
        description: "",
      });

      await loadData();
    } catch {
      setError("Failed to create service request");
    }
  };

  return (
    <div>
      <h1>Requests</h1>
      <p>Create and view service requests.</p>

      {error && <p className="error">{error}</p>}
      {successMessage && <p className="success">{successMessage}</p>}

      <div className="card">
        <h2>Create request</h2>

        <form onSubmit={handleSubmit} className="form">
          <label>Client</label>
          <select
            name="clientId"
            value={formData.clientId}
            onChange={handleChange}
            required
          >
            <option value="">Select a client</option>
            {users.map((user) => (
              <option key={user.id} value={user.id}>
                {user.fullName} - {user.role}
              </option>
            ))}
          </select>

          <label>Service</label>
          <select
            name="serviceId"
            value={formData.serviceId}
            onChange={handleChange}
            required
          >
            <option value="">Select a service</option>
            {services.map((service) => (
              <option key={service.id} value={service.id}>
                {service.name}
              </option>
            ))}
          </select>

          <label>Title</label>
          <input
            name="title"
            value={formData.title}
            onChange={handleChange}
            placeholder="Request title"
            required
          />

          <label>Description</label>
          <textarea
            name="description"
            value={formData.description}
            onChange={handleChange}
            placeholder="Request description"
            required
          />

          <button type="submit" className="primary-button">
            Create request
          </button>
        </form>
      </div>

      <div className="card">
        <h2>Requests list</h2>

        {requests.length === 0 ? (
          <p>No requests found.</p>
        ) : (
          <table>
            <thead>
              <tr>
                <th>Client</th>
                <th>Service</th>
                <th>Title</th>
                <th>Status</th>
              </tr>
            </thead>

            <tbody>
              {requests.map((request) => (
                <tr key={request.id}>
                  <td>{request.clientName}</td>
                  <td>{request.serviceName}</td>
                  <td>{request.title}</td>
                  <td>{request.status}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}

export default Requests;
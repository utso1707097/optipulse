import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { Provider } from "react-redux";
import { BrowserRouter } from "react-router-dom";
import "./index.css";
import App from "./App";
import { store } from "./store";
import { installAuthBridge } from "./store/authSlice";

// Wires the API client's token access and silent refresh to the auth slice. Done here, once,
// at the composition root — the client must not import the store, or the two form a cycle.
installAuthBridge(store);

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <Provider store={store}>
      <BrowserRouter>
        <App />
      </BrowserRouter>
    </Provider>
  </StrictMode>,
);

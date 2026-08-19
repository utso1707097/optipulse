import { combineReducers, configureStore } from "@reduxjs/toolkit";
import auth from "./authSlice";
import flags from "./flagsSlice";
import experiments from "./experimentsSlice";
import analytics from "./analyticsSlice";

const rootReducer = combineReducers({ auth, flags, experiments, analytics });

export type RootState = ReturnType<typeof rootReducer>;

/** Tests build their own store with a preloaded slice rather than driving the real one through
 *  a login, so a reducer can be exercised in isolation from the network. */
export function createAppStore(preloadedState?: Partial<RootState>) {
  return configureStore({ reducer: rootReducer, preloadedState });
}

export const store = createAppStore();

export type AppStore = ReturnType<typeof createAppStore>;
export type AppDispatch = AppStore["dispatch"];

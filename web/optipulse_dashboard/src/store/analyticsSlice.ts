/**
 * Analytics slice (T058/T061) — read-only exposure and conversion counts per flag.
 *
 * Keyed by flag so switching flags does not show the previous flag's numbers while the new
 * request is in flight. Attributing one experiment's conversion rate to another is the single
 * most consequential mistake this screen could make.
 */
import { createAsyncThunk, createSlice } from "@reduxjs/toolkit";
import {
  ApiError,
  analyticsApi,
  type FlagExposureResponse,
} from "../api/client";

export interface AnalyticsState {
  selectedFlagKey: string | null;
  byFlagKey: Record<string, FlagExposureResponse>;
  loading: boolean;
  error: string | null;
}

const initialState: AnalyticsState = {
  selectedFlagKey: null,
  byFlagKey: {},
  loading: false,
  error: null,
};

export const fetchExposures = createAsyncThunk<
  FlagExposureResponse,
  string,
  { rejectValue: string }
>("analytics/exposures", async (flagKey, { rejectWithValue, signal }) => {
  try {
    return await analyticsApi.exposures(flagKey, signal);
  } catch (error) {
    if (error instanceof ApiError) {
      return rejectWithValue(
        error.isNetworkFailure
          ? "Could not reach the OptiPulse API."
          : error.message,
      );
    }
    return rejectWithValue("Could not load analytics.");
  }
});

const analyticsSlice = createSlice({
  name: "analytics",
  initialState,
  reducers: {
    flagSelected(state, action: { payload: string | null; type: string }) {
      state.selectedFlagKey = action.payload;
      state.error = null;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchExposures.pending, (state) => {
        state.loading = true;
      })
      .addCase(fetchExposures.fulfilled, (state, action) => {
        state.loading = false;
        state.error = null;
        state.byFlagKey[action.payload.flagKey] = action.payload;
      })
      .addCase(fetchExposures.rejected, (state, action) => {
        state.loading = false;
        // Drop the cached figures for the flag being viewed: numbers that may be stale are
        // worse than no numbers when the decision they inform is "ship this to everyone".
        if (state.selectedFlagKey)
          delete state.byFlagKey[state.selectedFlagKey];
        state.error = action.payload ?? "Could not load analytics.";
      });
  },
});

export const { flagSelected } = analyticsSlice.actions;
export default analyticsSlice.reducer;

/**
 * Experiments slice (T058). Same contract as flagsSlice: fetched data for rendering, every
 * mutation followed by a server re-read rather than a local patch.
 */
import { createAsyncThunk, createSlice } from "@reduxjs/toolkit";
import {
  ApiError,
  experimentsApi,
  type CreateExperimentRequest,
  type ExperimentResponse,
  type UpdateExperimentRequest,
} from "../api/client";

export interface ExperimentsState {
  items: ExperimentResponse[];
  /** The flagKey filter the current `items` were read under — null means "all experiments". */
  filterFlagKey: string | null;
  loading: boolean;
  error: string | null;
  mutationError: string | null;
  mutating: boolean;
  conflict: boolean;
}

const initialState: ExperimentsState = {
  items: [],
  filterFlagKey: null,
  loading: false,
  error: null,
  mutationError: null,
  mutating: false,
  conflict: false,
};

function describe(error: unknown): string {
  if (error instanceof ApiError) {
    if (error.isNetworkFailure) return "Could not reach the OptiPulse API.";
    if (error.status === 403) return "Your role does not permit this action.";
    if (error.isVersionConflict)
      return "Someone else changed this experiment while you were editing. Reload, then re-apply your change.";
    return error.message;
  }
  return "Something went wrong.";
}

type MutationRejection = { message: string; conflict: boolean };

function rejection(error: unknown): MutationRejection {
  return {
    message: describe(error),
    conflict: error instanceof ApiError && error.isVersionConflict,
  };
}

export const fetchExperiments = createAsyncThunk<
  { items: ExperimentResponse[]; flagKey: string | null },
  string | undefined,
  { rejectValue: string }
>("experiments/fetch", async (flagKey, { rejectWithValue, signal }) => {
  try {
    return {
      items: await experimentsApi.list(flagKey, signal),
      flagKey: flagKey ?? null,
    };
  } catch (error) {
    return rejectWithValue(describe(error));
  }
});

/** Re-reads under the filter that is currently in effect, so a create does not silently widen it. */
function reread(getState: () => unknown) {
  const state = getState() as { experiments: ExperimentsState };
  return state.experiments.filterFlagKey ?? undefined;
}

export const createExperiment = createAsyncThunk<
  void,
  CreateExperimentRequest,
  { rejectValue: MutationRejection }
>(
  "experiments/create",
  async (body, { dispatch, getState, rejectWithValue }) => {
    try {
      await experimentsApi.create(body);
      await dispatch(fetchExperiments(reread(getState)));
    } catch (error) {
      return rejectWithValue(rejection(error));
    }
  },
);

export const updateExperiment = createAsyncThunk<
  void,
  { id: string; version: number | string; body: UpdateExperimentRequest },
  { rejectValue: MutationRejection }
>(
  "experiments/update",
  async ({ id, version, body }, { dispatch, getState, rejectWithValue }) => {
    try {
      await experimentsApi.update(id, version, body);
      await dispatch(fetchExperiments(reread(getState)));
    } catch (error) {
      return rejectWithValue(rejection(error));
    }
  },
);

export const changeExperimentStatus = createAsyncThunk<
  void,
  { id: string; status: string },
  { rejectValue: MutationRejection }
>(
  "experiments/status",
  async ({ id, status }, { dispatch, getState, rejectWithValue }) => {
    try {
      await experimentsApi.changeStatus(id, status);
      await dispatch(fetchExperiments(reread(getState)));
    } catch (error) {
      return rejectWithValue(rejection(error));
    }
  },
);

const experimentsSlice = createSlice({
  name: "experiments",
  initialState,
  reducers: {
    mutationErrorDismissed(state) {
      state.mutationError = null;
      state.conflict = false;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchExperiments.pending, (state) => {
        state.loading = true;
      })
      .addCase(fetchExperiments.fulfilled, (state, action) => {
        state.loading = false;
        state.error = null;
        state.items = action.payload.items;
        state.filterFlagKey = action.payload.flagKey;
      })
      .addCase(fetchExperiments.rejected, (state, action) => {
        state.loading = false;
        state.items = [];
        state.error = action.payload ?? "Could not load experiments.";
      });

    for (const thunk of [
      createExperiment,
      updateExperiment,
      changeExperimentStatus,
    ]) {
      builder
        .addCase(thunk.pending, (state) => {
          state.mutating = true;
          state.mutationError = null;
          state.conflict = false;
        })
        .addCase(thunk.fulfilled, (state) => {
          state.mutating = false;
        })
        .addCase(thunk.rejected, (state, action) => {
          state.mutating = false;
          state.mutationError =
            action.payload?.message ?? "The change could not be saved.";
          state.conflict = action.payload?.conflict ?? false;
        });
    }
  },
});

export const { mutationErrorDismissed } = experimentsSlice.actions;
export default experimentsSlice.reducer;

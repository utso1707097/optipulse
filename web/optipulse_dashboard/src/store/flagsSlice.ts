/**
 * Flags slice (T058).
 *
 * THE RULE THIS SLICE FOLLOWS (constitution v2.4.0): the store holds fetched data for
 * RENDERING. It is not a second source of truth. So every mutation re-reads the list from the
 * server instead of patching the local copy optimistically.
 *
 * That costs one extra round trip and is worth it here: flags are edited concurrently by
 * several people and the server can reject an edit on a version conflict. A locally-patched
 * list would show the change that the server just refused.
 */
import { createAsyncThunk, createSlice } from "@reduxjs/toolkit";
import {
  ApiError,
  flagsApi,
  type CreateFlagRequest,
  type FlagResponse,
  type UpdateFlagRequest,
} from "../api/client";

export interface FlagsState {
  items: FlagResponse[];
  loading: boolean;
  /** Set when the last read failed. Cleared on the next successful read. */
  error: string | null;
  /** Set when the last WRITE failed. Kept apart from `error` so a failed save does not read
   *  as "the list could not be loaded" — the list on screen is still valid. */
  mutationError: string | null;
  mutating: boolean;
  /** True when the last failure was a lost-update rejection, so the UI can say so specifically. */
  conflict: boolean;
}

const initialState: FlagsState = {
  items: [],
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
      return "Someone else changed this flag while you were editing. Reload to see the current version, then re-apply your change.";
    return error.message;
  }
  return "Something went wrong.";
}

export const fetchFlags = createAsyncThunk<
  FlagResponse[],
  void,
  { rejectValue: string }
>("flags/fetch", async (_, { rejectWithValue, signal }) => {
  try {
    return await flagsApi.list(signal);
  } catch (error) {
    return rejectWithValue(describe(error));
  }
});

/**
 * Mutations return void and re-dispatch fetchFlags: the caller renders the SERVER's list, not
 * the response of its own write. `rejectValue` carries both the message and whether it was a
 * conflict, because those get different remedies in the UI.
 */
type MutationRejection = { message: string; conflict: boolean };

function rejection(error: unknown): MutationRejection {
  return {
    message: describe(error),
    conflict: error instanceof ApiError && error.isVersionConflict,
  };
}

export const createFlag = createAsyncThunk<
  void,
  CreateFlagRequest,
  { rejectValue: MutationRejection }
>("flags/create", async (body, { dispatch, rejectWithValue }) => {
  try {
    await flagsApi.create(body);
    await dispatch(fetchFlags());
  } catch (error) {
    return rejectWithValue(rejection(error));
  }
});

export const updateFlag = createAsyncThunk<
  void,
  { key: string; version: number | string; body: UpdateFlagRequest },
  { rejectValue: MutationRejection }
>(
  "flags/update",
  async ({ key, version, body }, { dispatch, rejectWithValue }) => {
    try {
      await flagsApi.update(key, version, body);
      await dispatch(fetchFlags());
    } catch (error) {
      return rejectWithValue(rejection(error));
    }
  },
);

export const changeFlagStatus = createAsyncThunk<
  void,
  { key: string; status: string },
  { rejectValue: MutationRejection }
>("flags/status", async ({ key, status }, { dispatch, rejectWithValue }) => {
  try {
    await flagsApi.changeStatus(key, status);
    await dispatch(fetchFlags());
  } catch (error) {
    return rejectWithValue(rejection(error));
  }
});

export const setKillSwitch = createAsyncThunk<
  void,
  { key: string; engaged: boolean },
  { rejectValue: MutationRejection }
>(
  "flags/killSwitch",
  async ({ key, engaged }, { dispatch, rejectWithValue }) => {
    try {
      await flagsApi.setKillSwitch(key, engaged);
      await dispatch(fetchFlags());
    } catch (error) {
      return rejectWithValue(rejection(error));
    }
  },
);

const flagsSlice = createSlice({
  name: "flags",
  initialState,
  reducers: {
    mutationErrorDismissed(state) {
      state.mutationError = null;
      state.conflict = false;
    },
  },
  extraReducers: (builder) => {
    builder
      .addCase(fetchFlags.pending, (state) => {
        state.loading = true;
      })
      .addCase(fetchFlags.fulfilled, (state, action) => {
        state.loading = false;
        state.error = null;
        state.items = action.payload;
      })
      .addCase(fetchFlags.rejected, (state, action) => {
        state.loading = false;
        // The previously loaded list is DISCARDED on a read failure. Leaving it on screen next
        // to an error banner is how someone ends up operating a kill-switch against a picture
        // of the system as it was ten minutes ago (T062).
        state.items = [];
        state.error = action.payload ?? "Could not load flags.";
      });

    for (const thunk of [
      createFlag,
      updateFlag,
      changeFlagStatus,
      setKillSwitch,
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

export const { mutationErrorDismissed } = flagsSlice.actions;
export default flagsSlice.reducer;

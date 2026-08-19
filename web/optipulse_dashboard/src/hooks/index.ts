/**
 * Typed react-redux hooks (T058). Components import these, never the untyped originals, so a
 * selector that reads a field which no longer exists fails at compile time.
 */
import { useDispatch, useSelector } from "react-redux";
import type { AppDispatch, RootState } from "../store";

export const useAppDispatch = useDispatch.withTypes<AppDispatch>();
export const useAppSelector = useSelector.withTypes<RootState>();

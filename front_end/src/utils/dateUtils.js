export const toIsoDateTime = (date) => {
    if (!date) return "";
    return new Date(date).toISOString();
};
